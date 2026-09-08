import { useEffect, useState } from 'react'
import type { Sensor } from '../models/sensor'
import {
  createBoolTelemetry,
  createFloatTelemetry,
  createIntTelemetry,
  getSensors,
} from '../services/apiService'
import './TelemetryPage.css'

interface TelemetryPageProps {
  onBack: () => void
}

function TelemetryPage({
  onBack,
}: TelemetryPageProps) {
  const [sensors, setSensors] = useState<Sensor[]>([])
  const [sensorId, setSensorId] = useState('')
  const [metric, setMetric] = useState('')
  const [value, setValue] = useState('')
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')

  useEffect(() => {
    async function loadSensors() {
      try {
        const data = await getSensors()

        setSensors(data)

        if (data.length > 0) {
          setSensorId(data[0].id)
        }
      } catch {
        setError('Unable to load registered sensors.')
      }
    }

    loadSensors()
  }, [])

  const selectedSensor =
    sensors.find((sensor) => sensor.id === sensorId)

  async function handleSubmit(
    event: React.FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    setMessage('')
    setError('')

    if (!selectedSensor) {
      setError('Select a registered sensor.')
      return
    }

    if (!metric.trim()) {
      setError('Enter a telemetry metric.')
      return
    }

    try {
      if (selectedSensor.category === 'Environmental') {
        const numericValue = Number.parseFloat(value)

        if (Number.isNaN(numericValue)) {
          setError('Environmental telemetry requires a number.')
          return
        }

        await createFloatTelemetry({
          sensorId,
          metric,
          value: numericValue,
        })
      }

      if (selectedSensor.category === 'PowerConsumption') {
        const numericValue = Number.parseInt(value, 10)

        if (Number.isNaN(numericValue)) {
          setError('Power telemetry requires an integer.')
          return
        }

        await createIntTelemetry({
          sensorId,
          metric,
          value: numericValue,
        })
      }

      if (selectedSensor.category === 'Actuator') {
        if (
          value.toLowerCase() !== 'true' &&
          value.toLowerCase() !== 'false'
        ) {
          setError(
            'Actuator telemetry must be true or false.',
          )
          return
        }

        await createBoolTelemetry({
          sensorId,
          metric,
          value: value.toLowerCase() === 'true',
        })
      }

      setMessage('Telemetry submitted successfully.')
      setValue('')
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Unable to submit telemetry.',
      )
    }
  }

  return (
    <div className="telemetry-page">
      <header className="telemetry-header">
        <button
          type="button"
          className="telemetry-back"
          onClick={onBack}
        >
          ← Back to Sensors
        </button>

        <p>SMART-X / TELEMETRY</p>

        <h1>Telemetry Ingestion</h1>

        <span>
          Submit strongly typed telemetry readings to the
          Smart-X gateway.
        </span>
      </header>

      <main className="telemetry-main">
        <section className="telemetry-panel">
          <h2>Submit Telemetry</h2>

          {error && (
            <div className="telemetry-error">
              {error}
            </div>
          )}

          {message && (
            <div className="telemetry-success">
              {message}
            </div>
          )}

          <form onSubmit={handleSubmit}>
            <label htmlFor="sensor">
              Sensor
            </label>

            <select
              id="sensor"
              value={sensorId}
              onChange={(event) =>
                setSensorId(event.target.value)
              }
            >
              {sensors.map((sensor) => (
                <option
                  value={sensor.id}
                  key={sensor.id}
                >
                  {sensor.deviceIdentifier}
                  {' — '}
                  {sensor.category}
                </option>
              ))}
            </select>

            <label htmlFor="metric">
              Metric
            </label>

            <input
              id="metric"
              value={metric}
              onChange={(event) =>
                setMetric(event.target.value)
              }
              placeholder="e.g. Temperature"
            />

            <label htmlFor="value">
              Value
            </label>

            <input
              id="value"
              value={value}
              onChange={(event) =>
                setValue(event.target.value)
              }
              placeholder={
                selectedSensor?.category === 'Actuator'
                  ? 'true or false'
                  : 'Enter reading'
              }
            />

            {selectedSensor && (
              <div className="telemetry-type">
                Expected type:{' '}
                <strong>
                  {selectedSensor.category ===
                  'Environmental'
                    ? 'float'
                    : selectedSensor.category ===
                        'PowerConsumption'
                      ? 'int'
                      : 'bool'}
                </strong>
              </div>
            )}

            <button type="submit">
              Submit Telemetry
            </button>
          </form>
        </section>
      </main>
    </div>
  )
}

export default TelemetryPage