
import { useCallback, useEffect, useState } from 'react'
import {
  getActiveDeviceIssues,
  getDeviceAlertStatus,
  getTelemetryQueueStatus,
} from '../services/part2ApiService'
import './Part2Dashboard.css'

interface HealthSensor {
  sensorId: string
  deviceIdentifier: string
  category: string
  metric: string
  latestValue: string
  isConnected: boolean
  isAnomaly: boolean
  statusMessage: string
  latestTimestampUtc: string | null
}

interface HealthResponse {
  generatedAtUtc: string
  totalSensors: number
  connectedSensors: number
  anomalyCount: number
  activeIssues: number
  sensors: HealthSensor[]
}

interface HealthSummary {
  health: HealthResponse
  pendingAlerts: number
  processedReadings: number
}

export default function OperationalHealthPanel() {
  const [summary, setSummary] = useState<HealthSummary | null>(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const refresh = useCallback(async () => {
    try {
      const [healthResponse, alerts, queue, issues] =
        await Promise.all([
          fetch('https://localhost:7179/api/device-health'),
          getDeviceAlertStatus(),
          getTelemetryQueueStatus(),
          getActiveDeviceIssues(),
        ])

      if (!healthResponse.ok) {
        throw new Error(
          `Health API returned HTTP ${healthResponse.status}`,
        )
      }

      const health =
        await healthResponse.json() as HealthResponse

      setSummary({
        health: {
          ...health,
          activeIssues: issues.activeCount,
        },
        pendingAlerts: alerts.pendingCount,
        processedReadings: queue.processedCount,
      })

      setError('')
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Unable to load operational health.',
      )
    }
  }, [])

  useEffect(() => {
    void refresh()

    const timer = window.setInterval(() => {
      void refresh()
    }, 5000)

    return () => window.clearInterval(timer)
  }, [refresh])

  async function handleRefresh() {
    setLoading(true)
    await refresh()
    setLoading(false)
  }

  const health = summary?.health

  return (
    <div className="p2-stack">
      <section className="p2-panel">
        <h2>Operational Health Overview</h2>
        <p>
          Live sensor connectivity, anomaly status, fault tracking,
          and queue processing.
        </p>

        <div className="p2-grid">
          <div className="p2-stat">
            <span>Total sensors</span>
            <strong>{health?.totalSensors ?? 0}</strong>
          </div>

          <div className="p2-stat">
            <span>Connected sensors</span>
            <strong>{health?.connectedSensors ?? 0}</strong>
          </div>

          <div className="p2-stat">
            <span>Anomalous sensors</span>
            <strong>{health?.anomalyCount ?? 0}</strong>
          </div>

          <div className="p2-stat">
            <span>Active device issues</span>
            <strong>{health?.activeIssues ?? 0}</strong>
          </div>

          <div className="p2-stat">
            <span>Pending priority alerts</span>
            <strong>{summary?.pendingAlerts ?? 0}</strong>
          </div>

          <div className="p2-stat">
            <span>Processed telemetry readings</span>
            <strong>{summary?.processedReadings ?? 0}</strong>
          </div>
        </div>

        <div className="p2-actions">
          <button
            className="p2-secondary"
            onClick={() => void handleRefresh()}
            disabled={loading}
          >
            {loading ? 'Refreshing...' : 'Refresh Health'}
          </button>
        </div>

        {error && (
          <p className="p2-error" role="alert">{error}</p>
        )}
      </section>

      <section className="p2-panel">
        <h2>Device Status Register</h2>

        <div className="p2-table-wrap">
          <table className="p2-table">
            <thead>
              <tr>
                <th>Device</th>
                <th>Category</th>
                <th>Connectivity</th>
                <th>Latest metric</th>
                <th>Value</th>
                <th>Anomaly</th>
                <th>Status</th>
              </tr>
            </thead>

            <tbody>
              {health?.sensors.map(sensor => (
                <tr key={sensor.sensorId}>
                  <td>{sensor.deviceIdentifier}</td>
                  <td>{sensor.category}</td>
                  <td>
                    <span className="p2-pill">
                      {sensor.isConnected
                        ? 'Connected'
                        : 'Offline / Awaiting'}
                    </span>
                  </td>
                  <td>{sensor.metric}</td>
                  <td>{sensor.latestValue}</td>
                  <td>{sensor.isAnomaly ? 'Yes' : 'No'}</td>
                  <td>{sensor.statusMessage}</td>
                </tr>
              ))}

              {(!health || health.sensors.length === 0) && (
                <tr>
                  <td colSpan={7}>
                    No registered sensors available.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  )
}
