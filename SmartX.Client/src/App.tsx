
import { useState } from 'react'

import DashboardPage from './pages/DashboardPage'
import SensorManagementPage from './pages/SensorManagementPage'
import TelemetryPage from './pages/TelemetryPage'
import AttachmentsPage from './pages/AttachmentsPage'
import LiveTelemetryDashboardPage from './pages/LiveTelemetryDashboardPage'
import Part2Workspace from './pages/Part2Workspace'
import './pages/Part2Dashboard.css'

type AppView =
  | 'dashboard'
  | 'sensors'
  | 'telemetry'
  | 'attachments'
  | 'live'
  | 'part2'

function App() {
  const [view, setView] =
    useState<AppView>('dashboard')

  const [part2Visited, setPart2Visited] = useState(false)

  function navigate(next: AppView) {
    setView(next)

    if (next === 'part2') {
      setPart2Visited(true)
    }
  }

  function renderExistingPage() {
    if (view === 'telemetry') {
      return (
        <TelemetryPage
          onBack={() => navigate('sensors')}
        />
      )
    }

    if (view === 'attachments') {
      return (
        <AttachmentsPage
          onBack={() => navigate('sensors')}
        />
      )
    }

    if (view === 'live') {
      return (
        <LiveTelemetryDashboardPage
          onBack={() => navigate('sensors')}
        />
      )
    }

    if (view === 'sensors') {
      return (
        <SensorManagementPage
          onBack={() => navigate('dashboard')}
          onOpenTelemetry={() => navigate('telemetry')}
          onOpenAttachments={() => navigate('attachments')}
          onOpenLiveDashboard={() => navigate('live')}
        />
      )
    }

    return (
      <DashboardPage
        onOpenSensorModule={() => navigate('sensors')}
        onOpenCommandCentre={() => navigate('part2')}
      />
    )
  }

  return (
    <>
      <div hidden={view === 'part2'}>
        <div className="p2-global-nav">
          <strong>Smart-X</strong>

          <div className="p2-actions">
            <button
              onClick={() => navigate('dashboard')}
              className="p2-secondary"
            >
              Home
            </button>

            <button
              onClick={() => navigate('sensors')}
              className="p2-secondary"
            >
              Sensors
            </button>

            <button
              onClick={() => navigate('part2')}
            >
              Command Centre
            </button>
          </div>
        </div>

        {view !== 'part2' && renderExistingPage()}
      </div>

      {part2Visited && (
        <div hidden={view !== 'part2'}>
          <Part2Workspace
            onBack={() => navigate('dashboard')}
          />
        </div>
      )}
    </>
  )
}

export default App
