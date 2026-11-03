
import { useCallback, useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import type { Sensor } from '../models/sensor'
import type { HistoricalTelemetryEntry } from '../models/telemetryHistory'
import { getSensors } from '../services/apiService'
import { getMeshTelemetryRange } from '../services/meshTelemetryApiService'
import './MeshTopologyPage.css'
import './Part2Dashboard.css'

function formatLocalDateTime(date: Date): string {
  const pad = (value: number) => String(value).padStart(2, '0')

  return [
    date.getFullYear(),
    '-',
    pad(date.getMonth() + 1),
    '-',
    pad(date.getDate()),
    'T',
    pad(date.getHours()),
    ':',
    pad(date.getMinutes()),
  ].join('')
}

export default function MeshTelemetryHistoryPanel() {
  const [sensors, setSensors] = useState<Sensor[]>([])
  const [sensorId, setSensorId] = useState('')
  const [from, setFrom] = useState(() =>
    formatLocalDateTime(new Date(Date.now() - 86400000)),
  )
  const [to, setTo] = useState(() =>
    formatLocalDateTime(new Date()),
  )
  const [readings, setReadings] =
    useState<HistoricalTelemetryEntry[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [searched, setSearched] = useState(false)

  const loadSensors = useCallback(async () => {
    try {
      const result = await getSensors()
      setSensors(result)
      setSensorId(current =>
        result.some(sensor => sensor.id === current)
          ? current
          : result[0]?.id ?? '',
      )
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'Unable to load sensors.',
      )
    }
  }, [])

  useEffect(() => {
    const initialTimer = window.setTimeout(() => {
      void loadSensors()
    }, 0)

    return () => window.clearTimeout(initialTimer)
  }, [loadSensors])

  async function handleSearch(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (!sensorId) {
      setError('Please select a registered sensor.')
      return
    }

    const startDate = new Date(from)
    const endDate = new Date(to)

    if (
      !Number.isFinite(startDate.getTime()) ||
      !Number.isFinite(endDate.getTime())
    ) {
      setError('Please enter valid start and end dates.')
      return
    }

    if (startDate > endDate) {
      setError('The start date cannot be after the end date.')
      return
    }

    setLoading(true)
    setError('')
    setSearched(false)

    try {
      const result = await getMeshTelemetryRange(
        sensorId,
        startDate.toISOString(),
        endDate.toISOString(),
      )

      setReadings(result)
      setSearched(true)
    } catch (err) {
      setReadings([])
      setError(
        err instanceof Error
          ? err.message
          : 'Unable to retrieve historical telemetry.',
      )
    } finally {
      setLoading(false)
    }
  }

  const selectedSensor = sensors.find(
    sensor => sensor.id === sensorId,
  )

  return (
    <section className="mesh-card">
      <div className="mesh-section-heading">
        <div>
          <h2>Historical Sensor Telemetry</h2>
          <p>
            Query timestamped sensor readings using the
            self-balancing AVL tree.
          </p>
        </div>
        <button
          type="button"
          className="mesh-button mesh-button-secondary"
          onClick={() => void loadSensors()}
        >
          Refresh Sensors
        </button>
      </div>

      <form
        className="mesh-form"
        onSubmit={event => void handleSearch(event)}
      >
        <label>
          Select sensor
          <select
            value={sensorId}
            onChange={event => {
              setSensorId(event.target.value)
              setReadings([])
              setSearched(false)
            }}
          >
            {sensors.length === 0 && (
              <option value="">No registered sensors</option>
            )}
            {sensors.map(sensor => (
              <option key={sensor.id} value={sensor.id}>
                {sensor.nodeId} — {sensor.deviceIdentifier}
              </option>
            ))}
          </select>
        </label>

        <label>
          Start date and time
          <input
            type="datetime-local"
            value={from}
            onChange={event => {
              setFrom(event.target.value)
              setSearched(false)
            }}
            required
          />
        </label>

        <label>
          End date and time
          <input
            type="datetime-local"
            value={to}
            onChange={event => {
              setTo(event.target.value)
              setSearched(false)
            }}
            required
          />
        </label>

        <div style={{ display: 'flex', alignItems: 'end' }}>
          <button
            className="mesh-button mesh-button-primary"
            type="submit"
            disabled={loading || !sensorId}
          >
            {loading ? 'Searching...' : 'Search AVL History'}
          </button>
        </div>
      </form>

      {error && (
        <div className="mesh-alert mesh-alert-error" role="alert">
          {error}
        </div>
      )}

      <div style={{ marginTop: 24 }}>
        {selectedSensor && (
          <p>
            <strong>Selected device:</strong>{' '}
            {selectedSensor.deviceIdentifier}
          </p>
        )}

        {searched && (
          <p>
            <strong>{readings.length}</strong> historical readings
            returned for the selected time range.
          </p>
        )}

        <div className="p2-table-wrap">
          <table className="p2-table">
            <thead>
              <tr>
                <th>Timestamp</th>
                <th>Metric</th>
                <th>Value</th>
                <th>Data Type</th>
              </tr>
            </thead>
            <tbody>
              {readings.map(entry => (
                <tr key={entry.id}>
                  <td>
                    {new Date(entry.timestampUtc).toLocaleString()}
                  </td>
                  <td>{entry.metric}</td>
                  <td>{entry.value}</td>
                  <td>{entry.valueType}</td>
                </tr>
              ))}

              {readings.length === 0 && (
                <tr>
                  <td colSpan={4}>
                    {searched
                      ? 'No readings found in this time range.'
                      : 'Select a sensor and search its telemetry history.'}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </section>
  )
}
