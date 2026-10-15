
import { useCallback, useEffect, useState } from 'react'
import type { Sensor } from '../models/sensor'
import type {
  AlertSeverity,
  DeviceAlert,
  DeviceAlertStatus,
} from '../models/deviceAlert'
import { getSensors } from '../services/apiService'
import {
  enqueueDeviceAlert,
  getDeviceAlertStatus,
  processNextAlert,
} from '../services/part2ApiService'
import './Part2Dashboard.css'

const severities: AlertSeverity[] = [
  'Critical',
  'High',
  'Medium',
  'Low',
]

export default function PriorityAlertsPanel() {
  const [sensors, setSensors] = useState<Sensor[]>([])
  const [sensorId, setSensorId] = useState('')
  const [severity, setSeverity] =
    useState<AlertSeverity>('High')
  const [message, setMessage] = useState('')
  const [status, setStatus] = useState<DeviceAlertStatus>({
    pendingCount: 0,
    processed: [],
  })
  const [lastProcessed, setLastProcessed] =
    useState<DeviceAlert | null>(null)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')

  const refresh = useCallback(async () => {
    try {
      setStatus(await getDeviceAlertStatus())
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unable to load alerts.')
    }
  }, [])

  useEffect(() => {
    async function load() {
      try {
        const result = await getSensors()
        setSensors(result)
        setSensorId(current =>
          result.some(s => s.id === current)
            ? current : result[0]?.id ?? '',
        )
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Unable to load sensors.')
      }
    }
    void load()
  }, [])

  useEffect(() => {
    void refresh()
    const timer = window.setInterval(() => {
      void refresh()
    }, 5000)
    return () => window.clearInterval(timer)
  }, [refresh])

  async function enqueue() {
    if (!sensorId || !message.trim() || busy) return

    setBusy(true)
    setError('')
    setNotice('')

    try {
      await enqueueDeviceAlert({
        sensorId,
        severity,
        message: message.trim(),
      })
      setNotice(`${severity} alert added to priority queue.`)
      setMessage('')
      await refresh()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unable to add alert.')
    } finally {
      setBusy(false)
    }
  }

  async function process() {
    setBusy(true)
    setError('')
    setNotice('')

    try {
      const result = await processNextAlert()
      setLastProcessed(result)
      setNotice(
        result
          ? `Processed ${result.severity} alert: ${result.message}`
          : 'No alerts are pending.',
      )
      await refresh()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Processing failed.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="p2-stack">
      <section className="p2-panel">
        <h2>Priority Alert Queue</h2>
        <p>
          Critical alerts are processed before high, medium, and low alerts.
          Equal-priority alerts retain their arrival order.
        </p>

        <div className="p2-grid">
          <div className="p2-stat">
            <span>Pending alerts</span>
            <strong>{status.pendingCount}</strong>
          </div>
          <div className="p2-stat">
            <span>Processed alerts</span>
            <strong>{status.processed.length}</strong>
          </div>
        </div>

        <div className="p2-grid">
          <label>
            Sensor
            <select value={sensorId}
              onChange={e => setSensorId(e.target.value)}>
              {sensors.length === 0 &&
                <option value="">No registered sensors</option>}
              {sensors.map(sensor => (
                <option value={sensor.id} key={sensor.id}>
                  {sensor.deviceIdentifier}
                </option>
              ))}
            </select>
          </label>

          <label>
            Severity
            <select value={severity}
              onChange={e =>
                setSeverity(e.target.value as AlertSeverity)}>
              {severities.map(value => (
                <option key={value} value={value}>{value}</option>
              ))}
            </select>
          </label>
        </div>

        <div className="p2-grid">
          <label>
            Alert message
            <input value={message}
              onChange={e => setMessage(e.target.value)}
              maxLength={300}
              placeholder="Describe the sensor issue" />
          </label>
        </div>

        <div className="p2-actions">
          <button onClick={() => void enqueue()}
            disabled={!sensorId || !message.trim() || busy}>
            Enqueue Alert
          </button>
          <button onClick={() => void process()}
            disabled={busy || status.pendingCount === 0}>
            Process Highest Priority
          </button>
          <button className="p2-secondary"
            onClick={() => void refresh()}>
            Refresh
          </button>
        </div>

        {notice && <p className="p2-success" role="status">{notice}</p>}
        {error && <p className="p2-error" role="alert">{error}</p>}
      </section>

      {lastProcessed && (
        <section className="p2-panel">
          <h2>Last Processed Alert</h2>
          <p>
            <span className="p2-pill">{lastProcessed.severity}</span>
            {' '}{lastProcessed.message}
          </p>
        </section>
      )}

      <section className="p2-panel">
        <h2>Processed Alert History</h2>
        <div className="p2-table-wrap">
          <table className="p2-table">
            <thead>
              <tr>
                <th>Time</th>
                <th>Sensor</th>
                <th>Severity</th>
                <th>Message</th>
              </tr>
            </thead>
            <tbody>
              {[...status.processed].reverse().map(alert => (
                <tr key={alert.id}>
                  <td>{new Date(alert.createdAtUtc).toLocaleString()}</td>
                  <td>{sensors.find(s => s.id === alert.sensorId)
                    ?.deviceIdentifier ?? alert.sensorId.slice(0, 8)}</td>
                  <td><span className="p2-pill">{alert.severity}</span></td>
                  <td>{alert.message}</td>
                </tr>
              ))}
              {status.processed.length === 0 &&
                <tr><td colSpan={4}>No processed alerts yet.</td></tr>}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  )
}
