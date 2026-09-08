import { useState } from 'react'
import DashboardPage from './pages/DashboardPage'
import SensorManagementPage from './pages/SensorManagementPage'
import TelemetryPage from './pages/TelemetryPage'

type AppView =
  | 'dashboard'
  | 'sensors'
  | 'telemetry'

function App() {
  const [view, setView] =
    useState<AppView>('dashboard')

  if (view === 'telemetry') {
    return (
      <TelemetryPage
        onBack={() => setView('sensors')}
      />
    )
  }

  if (view === 'sensors') {
    return (
      <SensorManagementPage
        onBack={() => setView('dashboard')}
        onOpenTelemetry={() =>
          setView('telemetry')
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