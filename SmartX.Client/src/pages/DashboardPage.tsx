
import { useEffect, useState } from 'react'
import { getApiHealth } from '../services/apiService'
import './DashboardPage.css'

interface DashboardPageProps {
  onOpenSensorModule: () => void
  onOpenCommandCentre: () => void
  onOpenMeshRouting: () => void
}

function DashboardPage({
  onOpenSensorModule,
  onOpenCommandCentre,
  onOpenMeshRouting,
}: DashboardPageProps) {
  const [apiConnected, setApiConnected] = useState(false)
  const [apiChecked, setApiChecked] = useState(false)

  useEffect(() => {
    async function checkApi() {
      try {
        await getApiHealth()
        setApiConnected(true)
      } catch {
        setApiConnected(false)
      } finally {
        setApiChecked(true)
      }
    }

    void checkApi()
  }, [])

  return (
    <div className="dashboard-page">
      <header className="dashboard-header">
        <div>
          <p className="eyebrow">SMART-X</p>
          <h1>IoT Data Ingestion and Validation Gateway</h1>
          <p className="subtitle">
            Monitor, configure and manage Smart-X sensor telemetry.
          </p>
        </div>

        <div
          className={`api-status ${
            apiConnected ? 'api-online' : 'api-offline'
          }`}
        >
          <span className="status-dot"></span>

          {!apiChecked
            ? 'Checking API...'
            : apiConnected
              ? 'API Connected'
              : 'API Disconnected'}
        </div>
      </header>

      <main>
        <section className="intro-section">
          <h2>Gateway Modules</h2>
          <p>
            Select an available Smart-X module to continue.
          </p>
        </section>

        <section className="module-grid">
          <article className="module-card active-card">
            <div className="module-number">01</div>

            <div className="module-content">
              <span className="module-status active-status">
                ACTIVE — PART 1
              </span>

              <h3>Sensor Data Ingestion and Telemetry</h3>

              <p>
                Register Smart-X sensors, ingest telemetry data and monitor
                sensor activity.
              </p>

              <button
                className="primary-button"
                type="button"
                onClick={onOpenSensorModule}
              >
                Open Module
              </button>
            </div>
          </article>

          <article className="module-card active-card">
            <div className="module-number">02</div>

            <div className="module-content">
              <span className="module-status active-status">
                ACTIVE — PART 2
              </span>

              <h3>Real-Time Command Stream and History</h3>

              <p>
                Monitor live device operations, execute manual commands,
                review command history, process priority alerts and
                investigate predictive insights.
              </p>

              <button
                className="primary-button"
                type="button"
                onClick={onOpenCommandCentre}
              >
                Open Module
              </button>
            </div>
          </article>

          <article className="module-card active-card">
            <div className="module-number">03</div>

            <div className="module-content">
              <span className="module-status active-status">
                ACTIVE — PART 3
              </span>

              <h3>Network Topology and Mesh Routing</h3>

              <p>
                Visualise sensor connections, calculate minimum-cost
                networks, inspect gateway routes and simulate automatic
                recovery after sensor or link failures.
              </p>

              <button
                className="primary-button"
                type="button"
                onClick={onOpenMeshRouting}
              >
                Open Module
              </button>
            </div>
          </article>
        </section>
      </main>
    </div>
  )
}

export default DashboardPage
