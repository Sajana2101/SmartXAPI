
import { useCallback, useEffect, useState } from 'react'
import type { Sensor } from '../models/sensor'
import { getSensors } from '../services/apiService'
import './Part2Dashboard.css'

interface Recommendation {
  id: string
  sensorId: string
  deviceIdentifier: string
  title: string
  explanation: string
  suggestedCommand: string
  score: number
  priority: string
  evidence: string
  generatedAtUtc: string
}

interface BehaviorSummary {
  sensorId: string
  searchCount: number
  commandCount: number
  recentTelemetryCount: number
  consecutiveAnomalousReadings: number
  searchCommandCorrelations: number
  lastInteractionUtc: string | null
}

interface InsightsResponse {
  generatedAtUtc: string
  analysedSensorCount: number
  recordedSearchCount: number
  recordedCommandCount: number
  recommendations: Recommendation[]
  behavior: BehaviorSummary[]
}

const API = 'https://localhost:7179/api/predictive-insights'

async function readResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    let message = `Request failed: HTTP ${response.status}`

    try {
      const data = await response.json() as {
        message?: string
        title?: string
      }
      message = data.message ?? data.title ?? message
    } catch {
      message = `Request failed: HTTP ${response.status}`
    }

    throw new Error(message)
  }

  return response.json() as Promise<T>
}

export default function PredictiveInsightsPanel() {
  const [sensors, setSensors] = useState<Sensor[]>([])
  const [sensorId, setSensorId] = useState('')
  const [query, setQuery] = useState('')
  const [insights, setInsights] = useState<InsightsResponse | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')

  const refresh = useCallback(async () => {
    try {
      const response = await fetch(API)
      const result = await readResponse<InsightsResponse>(response)
      setInsights(result)
      setError('')
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'Unable to load insights.',
      )
    }
  }, [])

  useEffect(() => {
    async function loadSensors() {
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
    }

    void loadSensors()
  }, [])

  useEffect(() => {
    void refresh()

    const timer = window.setInterval(() => {
      void refresh()
    }, 5000)

    return () => window.clearInterval(timer)
  }, [refresh])

  async function recordSearch() {
    if (!query.trim() || loading) return

    setLoading(true)
    setError('')
    setNotice('')

    try {
      const response = await fetch(`${API}/search`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          query: query.trim(),
          sensorId: sensorId || null,
        }),
      })

      await readResponse(response)

      setNotice('Search activity recorded successfully.')
      setQuery('')
      await refresh()
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'Unable to record search.',
      )
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="p2-stack">
      <section className="p2-panel">
        <h2>Automated Insights</h2>
        <p>
          Recommendations are generated from live sensor status,
          telemetry history, search activity, and manual commands.
          Suggestions do not execute automatically.
        </p>

        <div className="p2-grid">
          <div className="p2-stat">
            <span>Sensors analysed</span>
            <strong>{insights?.analysedSensorCount ?? 0}</strong>
          </div>

          <div className="p2-stat">
            <span>Recorded searches</span>
            <strong>{insights?.recordedSearchCount ?? 0}</strong>
          </div>

          <div className="p2-stat">
            <span>Recorded commands</span>
            <strong>{insights?.recordedCommandCount ?? 0}</strong>
          </div>

          <div className="p2-stat">
            <span>Active recommendations</span>
            <strong>{insights?.recommendations.length ?? 0}</strong>
          </div>
        </div>

        <div className="p2-actions">
          <button className="p2-secondary"
            onClick={() => void refresh()}>
            Refresh Insights
          </button>
        </div>
      </section>

      <section className="p2-panel">
        <h2>Record Device Search</h2>
        <p>
          Search interactions are analysed alongside subsequent manual
          commands to detect repeated operational patterns.
        </p>

        <div className="p2-grid">
          <label>
            Associated device
            <select
              value={sensorId}
              onChange={event => setSensorId(event.target.value)}
            >
              <option value="">General search</option>
              {sensors.map(sensor => (
                <option key={sensor.id} value={sensor.id}>
                  {sensor.deviceIdentifier} — {sensor.category}
                </option>
              ))}
            </select>
          </label>

          <label>
            Search query
            <input
              value={query}
              maxLength={150}
              onChange={event => setQuery(event.target.value)}
              placeholder="Search for pump, moisture, or sensor"
            />
          </label>
        </div>

        <div className="p2-actions">
          <button
            disabled={!query.trim() || loading}
            onClick={() => void recordSearch()}
          >
            {loading ? 'Recording...' : 'Record Search'}
          </button>
        </div>

        {notice && <p className="p2-success" role="status">{notice}</p>}
        {error && <p className="p2-error" role="alert">{error}</p>}
      </section>

      <section className="p2-panel">
        <h2>Suggested Actions</h2>

        {!insights || insights.recommendations.length === 0 ? (
          <div className="p2-empty">
            No significant patterns detected yet.
            Submit telemetry or record repeated device interactions
            to generate recommendations.
          </div>
        ) : (
          <div className="p2-stack">
            {insights.recommendations.map(item => (
              <div className="p2-stat" key={item.id}>
                <div className="p2-header">
                  <div>
                    <span className="p2-pill">{item.priority}</span>
                    <h2>{item.title}</h2>
                    <strong>{item.deviceIdentifier}</strong>
                  </div>
                  <div className="p2-stat">
                    <span>Priority score</span>
                    <strong>{item.score}/100</strong>
                  </div>
                </div>

                <p>{item.explanation}</p>

                <p>
                  <strong>Evidence:</strong> {item.evidence}
                </p>

                <p>
                  <strong>Suggested action:</strong>{' '}
                  {item.suggestedCommand}
                </p>

                <p>
                  <small>
                    This is a recommendation only.
                    Review the device before taking action.
                  </small>
                </p>
              </div>
            ))}
          </div>
        )}
      </section>

      <section className="p2-panel">
        <h2>Historical Behaviour Analysis</h2>

        <div className="p2-table-wrap">
          <table className="p2-table">
            <thead>
              <tr>
                <th>Device</th>
                <th>Searches</th>
                <th>Commands</th>
                <th>Search-command patterns</th>
                <th>Recent readings</th>
                <th>Consecutive anomalies</th>
              </tr>
            </thead>
            <tbody>
              {insights?.behavior.map(item => (
                <tr key={item.sensorId}>
                  <td>
                    {sensors.find(sensor =>
                      sensor.id === item.sensorId
                    )?.deviceIdentifier ?? item.sensorId.slice(0, 8)}
                  </td>
                  <td>{item.searchCount}</td>
                  <td>{item.commandCount}</td>
                  <td>{item.searchCommandCorrelations}</td>
                  <td>{item.recentTelemetryCount}</td>
                  <td>{item.consecutiveAnomalousReadings}</td>
                </tr>
              ))}

              {(!insights || insights.behavior.length === 0) && (
                <tr>
                  <td colSpan={6}>
                    No device behaviour available yet.
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
