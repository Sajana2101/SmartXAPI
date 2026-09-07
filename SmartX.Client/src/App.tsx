import { useState } from 'react'
import DashboardPage from './pages/DashboardPage'
import SensorManagementPage from './pages/SensorManagementPage'

type AppView = 'dashboard' | 'sensors'

function App() {
  const [view, setView] = useState<AppView>('dashboard')

  if (view === 'sensors') {
    return (
      <SensorManagementPage
        onBack={() => setView('dashboard')}
      />
    )
  }

  return (
    <DashboardPage
      onOpenSensorModule={() => setView('sensors')}
    />
  )
}

export default App