
import { useCallback, useEffect, useState } from 'react'
import type { Sensor } from '../models/sensor'
import type {
  DeviceCommand,
  DeviceCommandType,
  DeviceOperatingState,
} from '../models/deviceCommand'
import { getSensors } from '../services/apiService'
import {
  getDeviceCommandHistory,
  getDeviceOperatingState,
  submitDeviceCommand,
  undoLastDeviceCommand,
} from '../services/part2ApiService'
import './Part2Dashboard.css'

const commands: DeviceCommandType[] = [
  'EnableDevice',
  'DisableDevice',
  'RestartDevice',
  'SetSamplingInterval',
  'AcknowledgeAlert',
]

export default function DeviceCommandDashboard() {
  const [sensors, setSensors] = useState<Sensor[]>([])
  const [sensorId, setSensorId] = useState('')
  const [type, setType] =
    useState<DeviceCommandType>('DisableDevice')
  const [interval, setInterval] = useState(30)
  const [state, setState] =
    useState<DeviceOperatingState | null>(null)
  const [history, setHistory] = useState<DeviceCommand[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')

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

  const refresh = useCallback(async () => {
    try {
      const result = await getDeviceCommandHistory()
      setHistory(result)

      if (sensorId) {
        setState(await getDeviceOperatingState(sensorId))
      } else {
        setState(null)
      }
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'Unable to refresh commands.',
      )
    }
  }, [sensorId])

  useEffect(() => {
    void loadSensors()
  }, [loadSensors])

  useEffect(() => {
    void refresh()
    const timer = window.setInterval(() => {
      void refresh()
    }, 5000)
    return () => window.clearInterval(timer)
  }, [refresh])

  async function execute() {
    if (!sensorId || loading) return

    setLoading(true)
    setError('')
    setMessage('')

    try {
      const result = await submitDeviceCommand({
        sensorId,
        type,
        samplingIntervalSeconds:
          type === 'SetSamplingInterval' ? interval : null,
      })
      setMessage(result.message ?? 'Command processed.')
      await refresh()
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'Command failed.',
      )
    } finally {
      setLoading(false)
    }
  }

  async function undo() {
    setLoading(true)
    setError('')
    setMessage('')

    try {
      const result = await undoLastDeviceCommand()
      setMessage(result.message ?? 'Command undone.')
      await refresh()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Undo failed.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="p2-stack">
      <section className="p2-panel">
        <h2>Manual Device Commands</h2>
        <p>Send simulated commands to registered Smart-X sensors.</p>

        <div className="p2-grid">
          <label>
            Device
            <select value={sensorId}
              onChange={e => setSensorId(e.target.value)}>
              {sensors.length === 0 && (
                <option value="">No registered sensors</option>
              )}
              {sensors.map(sensor => (
                <option key={sensor.id} value={sensor.id}>
                  {sensor.deviceIdentifier} — {sensor.category}
                </option>
              ))}
            </select>
          </label>

          <label>
            Command
            <select value={type}
              onChange={e =>
                setType(e.target.value as DeviceCommandType)}>
              {commands.map(command => (
                <option key={command} value={command}>
                  {command}
                </option>
              ))}
            </select>
          </label>

          {type === 'SetSamplingInterval' && (
            <label>
              Sampling interval (seconds)
              <input type="number" min={1} max={86400}
                value={interval}
                onChange={e => setInterval(Number(e.target.value))} />
            </label>
          )}
        </div>

        <div className="p2-actions">
          <button onClick={() => void execute()}
            disabled={!sensorId || loading ||
              (type === 'SetSamplingInterval' &&
                (!Number.isInteger(interval) ||
                  interval < 1 || interval > 86400))}>
            {loading ? 'Processing...' : 'Execute Command'}
          </button>
          <button className="p2-secondary"
            onClick={() => void undo()} disabled={loading}>
            Undo Last Reversible Command
          </button>
          <button className="p2-secondary"
            onClick={() => {
              void loadSensors()
              void refresh()
            }}>
            Refresh
          </button>
        </div>

        {message && <p className="p2-success" role="status">{message}</p>}
        {error && <p className="p2-error" role="alert">{error}</p>}
      </section>

      <section className="p2-panel">
        <h2>Selected Device State</h2>
        {state ? (
          <div className="p2-grid">
            <div className="p2-stat">
              <span>Operating status</span>
              <strong>{state.enabled ? 'Enabled' : 'Disabled'}</strong>
            </div>
            <div className="p2-stat">
              <span>Sampling interval</span>
              <strong>{state.samplingIntervalSeconds}s</strong>
            </div>
          </div>
        ) : (
          <p>Select a registered sensor to see its state.</p>
        )}
      </section>

      <section className="p2-panel">
        <h2>Command History</h2>
        <div className="p2-table-wrap">
          <table className="p2-table">
            <thead>
              <tr>
                <th>Time</th>
                <th>Device</th>
                <th>Command</th>
                <th>Status</th>
                <th>Message</th>
              </tr>
            </thead>
            <tbody>
              {history.map(command => (
                <tr key={command.id}>
                  <td>{new Date(command.createdAtUtc).toLocaleString()}</td>
                  <td>{sensors.find(s => s.id === command.sensorId)
                    ?.deviceIdentifier ?? command.sensorId.slice(0, 8)}</td>
                  <td>{command.type}</td>
                  <td>{command.status}</td>
                  <td>{command.message ?? '—'}</td>
                </tr>
              ))}
              {history.length === 0 && (
                <tr><td colSpan={5}>No commands recorded yet.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  )
}
