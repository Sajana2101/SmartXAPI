
import { useCallback, useEffect, useMemo, useState } from 'react'
import type { Sensor } from '../models/sensor'
import type {
  HistoricalTelemetryEntry,
  TelemetryQueueStatus,
} from '../models/telemetryHistory'
import { getSensors } from '../services/apiService'
import {
  getSortedTelemetryHistory,
  getTelemetryQueueStatus,
  processTelemetryQueue,
} from '../services/part2ApiService'
import './Part2Dashboard.css'

export default function TelemetryHistoryPanel() {
  const [sensors, setSensors] = useState<Sensor[]>([])
  const [sensorId, setSensorId] = useState('')
  const [search, setSearch] = useState('')
  const [history, setHistory] =
    useState<HistoricalTelemetryEntry[]>([])
  const [queue, setQueue] = useState<TelemetryQueueStatus>({
    pendingCount: 0,
    processedCount: 0,
  })
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [busy, setBusy] = useState(false)

  const refresh = useCallback(async () => {
    try {
      const [entries, status] = await Promise.all([
        getSortedTelemetryHistory(sensorId || undefined),
        getTelemetryQueueStatus(),
      ])
      setHistory(entries)
      setQueue(status)
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'Unable to load telemetry.',
      )
    }
  }, [sensorId])

  useEffect(() => {
    async function loadSensors() {
      try {
        setSensors(await getSensors())
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

  const filtered = useMemo(() => {
    const term = search.trim().toLowerCase()

    return history.filter(entry => {
      if (!term) return true
      const sensor = sensors.find(s => s.id === entry.sensorId)
      return [
        sensor?.deviceIdentifier ?? '',
        entry.metric,
        entry.value,
        entry.valueType,
      ].some(value => value.toLowerCase().includes(term))
    })
  }, [history, search, sensors])

  async function processPending() {
    setBusy(true)
    setError('')
    setNotice('')

    try {
      const result = await processTelemetryQueue()
      setNotice(`Processed ${result.processed} pending readings.`)
      await refresh()
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'Queue processing failed.',
      )
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="p2-stack">
      <section className="p2-panel">
        <h2>FIFO Telemetry Queue</h2>
        <p>
          Telemetry received through the API is queued and processed
          in arrival order.
        </p>
        <div className="p2-grid">
          <div className="p2-stat">
            <span>Pending readings</span>
            <strong>{queue.pendingCount}</strong>
          </div>
          <div className="p2-stat">
            <span>Processed readings</span>
            <strong>{queue.processedCount}</strong>
          </div>
        </div>
        <div className="p2-actions">
          <button disabled={busy || queue.pendingCount === 0}
            onClick={() => void processPending()}>
            Process Pending
          </button>
          <button className="p2-secondary"
            onClick={() => void refresh()}>
            Refresh
          </button>
        </div>
        {notice && <p className="p2-success" role="status">{notice}</p>}
        {error && <p className="p2-error" role="alert">{error}</p>}
      </section>

      <section className="p2-panel">
        <h2>Chronological Telemetry History</h2>
        <p>
          Latest processed readings are displayed first.
          Historical entries are indexed by the backend
          using SortedDictionary.
        </p>
        <div className="p2-grid">
          <label>
            Filter by sensor
            <select value={sensorId}
              onChange={e => setSensorId(e.target.value)}>
              <option value="">All sensors</option>
              {sensors.map(sensor => (
                <option key={sensor.id} value={sensor.id}>
                  {sensor.deviceIdentifier}
                </option>
              ))}
            </select>
          </label>
          <label>
            Search readings
            <input value={search}
              onChange={e => setSearch(e.target.value)}
              placeholder="Metric, value, or device identifier" />
          </label>
        </div>
        <p>{filtered.length} matching readings</p>

        <div className="p2-table-wrap">
          <table className="p2-table">
            <thead>
              <tr>
                <th>Timestamp</th>
                <th>Sensor</th>
                <th>Metric</th>
                <th>Value</th>
                <th>Data type</th>
              </tr>
            </thead>
            <tbody>
              {filtered.slice(0, 200).map(entry => (
                <tr key={entry.id}>
                  <td>{new Date(entry.timestampUtc).toLocaleString()}</td>
                  <td>{sensors.find(s => s.id === entry.sensorId)
                    ?.deviceIdentifier ?? entry.sensorId.slice(0, 8)}</td>
                  <td>{entry.metric}</td>
                  <td>{entry.value}</td>
                  <td>{entry.valueType}</td>
                </tr>
              ))}
              {filtered.length === 0 &&
                <tr><td colSpan={5}>No matching telemetry yet.</td></tr>}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  )
}
