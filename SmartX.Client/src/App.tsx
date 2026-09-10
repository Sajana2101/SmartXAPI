import { useState } from 'react'

import DashboardPage
  from './pages/DashboardPage'

import SensorManagementPage
  from './pages/SensorManagementPage'

import TelemetryPage
  from './pages/TelemetryPage'

import AttachmentsPage
  from './pages/AttachmentsPage'

import LiveTelemetryDashboardPage
  from './pages/LiveTelemetryDashboardPage'

type AppView =
  | 'dashboard'
  | 'sensors'
  | 'telemetry'
  | 'attachments'
  | 'live'

function App() {
  const [view, setView] =
    useState<AppView>('dashboard')

  if (view === 'telemetry') {
    return (
      <TelemetryPage
        onBack={() =>
          setView('sensors')
        }
      />
    )
  }

  if (view === 'attachments') {
    return (
      <AttachmentsPage
        onBack={() =>
          setView('sensors')
        }
      />
    )
  }

  if (view === 'live') {
    return (
      <LiveTelemetryDashboardPage
        onBack={() =>
          setView('sensors')
        }
      />
    )
  }

  if (view === 'sensors') {
    return (
      <SensorManagementPage
        onBack={() =>
          setView('dashboard')
        }

        onOpenTelemetry={() =>
          setView('telemetry')
        }

        onOpenAttachments={() =>
          setView('attachments')
        }

        onOpenLiveDashboard={() =>
          setView('live')
        }
      />
    )
  }

  return (
    <DashboardPage
      onOpenSensorModule={() =>
        setView('sensors')
      }
    />
  )
}

export default App