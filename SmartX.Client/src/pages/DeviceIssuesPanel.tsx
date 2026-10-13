
import { useCallback, useEffect, useMemo, useState } from 'react'
import type { Sensor } from '../models/sensor'
import type {
  DeviceIssueResponse,
  DeviceIssueType,
} from '../models/deviceIssue'
import { getSensors } from '../services/apiService'
import {
  getActiveDeviceIssues,
  reportDeviceIssue,
  resolveDeviceIssue,
} from '../services/part2ApiService'
import './Part2Dashboard.css'

const issueTypes: DeviceIssueType[] = ['Error', 'Disconnected']

export default function DeviceIssuesPanel() {
  const [sensors, setSensors] = useState<Sensor[]>([])
  const [sensorId, setSensorId] = useState('')
  const [issueType, setIssueType] =
    useState<DeviceIssueType>('Error')
  const [category, setCategory] = useState('All')
  const [response, setResponse] = useState<DeviceIssueResponse>({
    activeCount: 0,
    issues: [],
  })
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')

  const refresh = useCallback(async () => {
    try {
      const [sensorData, issueData] = await Promise.all([
        getSensors(),
        getActiveDeviceIssues(),
      ])

      setSensors(sensorData)
      setResponse(issueData)
      setSensorId(current =>
        sensorData.some(s => s.id === current)
          ? current : sensorData[0]?.id ?? '',
      )
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unable to refresh issues.')
    }
  }, [])

  useEffect(() => {
    void refresh()
    const timer = window.setInterval(() => {
      void refresh()
    }, 5000)
    return () => window.clearInterval(timer)
  }, [refresh])

  const categories = useMemo(
    () => ['All', ...new Set(sensors.map(s => s.category))],
    [sensors],
  )

  const filtered = useMemo(() => {
    return response.issues.filter(issue => {
      const sensor = sensors.find(s => s.id === issue.sensorId)
      return category === 'All' || sensor?.category === category
    })
  }, [response.issues, sensors, category])

  async function report() {
    if (!sensorId || busy) return
    setBusy(true)
    setError('')
    setNotice('')

    try {
      const result = await reportDeviceIssue(sensorId, issueType)
      setNotice(
        result.added
          ? 'Device issue reported.'
          : 'This device already has that active issue.',
      )
      await refresh()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Report failed.')
    } finally {
      setBusy(false)
    }
  }

  async function resolve(id: string, type: DeviceIssueType) {
    if (busy) return
    setBusy(true)
    setError('')
    setNotice('')

    try {
      const result = await resolveDeviceIssue(id, type)
      setNotice(
        result.resolved
          ? 'Issue resolved.'
          : 'Issue was already resolved.',
      )
      await refresh()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Resolution failed.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="p2-stack">
      <section className="p2-panel">
        <h2>Active Device Issues</h2>
        <div className="p2-grid">
          <div className="p2-stat">
            <span>Unique active issues</span>
            <strong>{response.activeCount}</strong>
          </div>
          <div className="p2-stat">
            <span>Visible after filtering</span>
            <strong>{filtered.length}</strong>
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
                <option key={sensor.id} value={sensor.id}>
                  {sensor.deviceIdentifier} — {sensor.category}
                </option>
              ))}
            </select>
          </label>
          <label>
            Issue type
            <select value={issueType}
              onChange={e =>
                setIssueType(e.target.value as DeviceIssueType)}>
              {issueTypes.map(value => (
                <option key={value} value={value}>{value}</option>
              ))}
            </select>
          </label>
        </div>

        <div className="p2-actions">
          <button disabled={!sensorId || busy}
            onClick={() => void report()}>
            Report Issue
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
        <h2>Issue Register</h2>
        <div className="p2-grid">
          <label>
            Filter by device category
            <select value={category}
              onChange={e => setCategory(e.target.value)}>
              {categories.map(value => (
                <option key={value} value={value}>{value}</option>
              ))}
            </select>
          </label>
        </div>

        <div className="p2-table-wrap">
          <table className="p2-table">
            <thead>
              <tr>
                <th>Sensor</th>
                <th>Category</th>
                <th>Issue</th>
                <th>Detected</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map(issue => {
                const sensor = sensors.find(s => s.id === issue.sensorId)
                return (
                  <tr key={`${issue.sensorId}-${issue.type}`}>
                    <td>{sensor?.deviceIdentifier ?? issue.sensorId}</td>
                    <td>{sensor?.category ?? 'Unknown'}</td>
                    <td><span className="p2-pill">{issue.type}</span></td>
                    <td>{new Date(
                      issue.firstDetectedAtUtc,
                    ).toLocaleString()}</td>
                    <td>
                      <button className="p2-primary"
                        disabled={busy}
                        onClick={() =>
                          void resolve(issue.sensorId, issue.type)}>
                        Resolve
                      </button>
                    </td>
                  </tr>
                )
              })}
              {filtered.length === 0 &&
                <tr><td colSpan={5}>No matching active issues.</td></tr>}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  )
}
