
import { useState } from 'react'
import DeviceCommandDashboard from './DeviceCommandDashboard'
import PriorityAlertsPanel from './PriorityAlertsPanel'
import DeviceIssuesPanel from './DeviceIssuesPanel'
import TelemetryHistoryPanel from './TelemetryHistoryPanel'
import PredictiveInsightsPanel from './PredictiveInsightsPanel'
import './Part2Dashboard.css'

type WorkspaceTab =
  | 'commands'
  | 'alerts'
  | 'issues'
  | 'history'
  | 'insights'

interface Part2WorkspaceProps {
  onBack: () => void
}

const tabs: {
  id: WorkspaceTab
  title: string
}[] = [
  { id: 'commands', title: 'Device Commands' },
  { id: 'alerts', title: 'Priority Alerts' },
  { id: 'issues', title: 'Device Issues' },
  { id: 'history', title: 'Telemetry History' },
  { id: 'insights', title: 'Automated Insights' },
]

export default function Part2Workspace({
  onBack,
}: Part2WorkspaceProps) {
  const [tab, setTab] = useState<WorkspaceTab>('commands')

  const [visited, setVisited] =
    useState<WorkspaceTab[]>(['commands'])

  function changeTab(next: WorkspaceTab) {
    setTab(next)

    setVisited(current =>
      current.includes(next)
        ? current
        : [...current, next],
    )
  }

  return (
    <main className="p2-workspace">
      <div className="p2-container">
        <header className="p2-header">
          <div>
            <h1>Smart-X Command Centre</h1>
            <p>
              Real-time device management, priority alerts,
              telemetry processing, and predictive insights
            </p>
          </div>

          <div className="p2-actions">
            <button
              className="p2-secondary"
              onClick={onBack}
            >
              Back to Smart-X
            </button>
          </div>
        </header>

        <nav
          className="p2-tabs"
          aria-label="Part 2 modules"
        >
          {tabs.map(item => (
            <button
              key={item.id}
              className={tab === item.id ? 'active' : ''}
              aria-current={
                tab === item.id ? 'page' : undefined
              }
              onClick={() => changeTab(item.id)}
            >
              {item.title}
            </button>
          ))}
        </nav>

        <div hidden={tab !== 'commands'}>
          <DeviceCommandDashboard />
        </div>

        {visited.includes('alerts') && (
          <div hidden={tab !== 'alerts'}>
            <PriorityAlertsPanel />
          </div>
        )}

        {visited.includes('issues') && (
          <div hidden={tab !== 'issues'}>
            <DeviceIssuesPanel />
          </div>
        )}

        {visited.includes('history') && (
          <div hidden={tab !== 'history'}>
            <TelemetryHistoryPanel />
          </div>
        )}

        {visited.includes('insights') && (
          <div hidden={tab !== 'insights'}>
            <PredictiveInsightsPanel />
          </div>
        )}
      </div>
    </main>
  )
}
