import {
  useCallback,
  useEffect,
  useState,
} from 'react'

import type {
  TelemetryDashboard,
} from '../models/telemetryDashboard'

import {
  getTelemetryDashboard,
  seedTelemetry,
} from '../services/apiService'

import TelemetrySparkline
  from '../components/TelemetrySparkline'

import './LiveTelemetryDashboardPage.css'

interface LiveTelemetryDashboardPageProps {
  onBack: () => void
}

function LiveTelemetryDashboardPage({
  onBack,
}: LiveTelemetryDashboardPageProps) {
  const [dashboard, setDashboard] =
    useState<TelemetryDashboard | null>(
      null,
    )

  const [loading, setLoading] =
    useState(true)

  const [generating, setGenerating] =
    useState(false)

  const [error, setError] =
    useState('')

  const loadDashboard =
    useCallback(async () => {
      try {
        const data =
          await getTelemetryDashboard()

        setDashboard(data)
        setError('')
      } catch (err) {
        setError(
          err instanceof Error
            ? err.message
            : 'Unable to load telemetry dashboard.',
        )
      } finally {
        setLoading(false)
      }
    }, [])

  useEffect(() => {
    loadDashboard()

    const interval =
      window.setInterval(
        loadDashboard,
        5000,
      )

    return () =>
      window.clearInterval(
        interval,
      )
  }, [loadDashboard])

  async function generateReading() {
    try {
      setGenerating(true)
      setError('')

      await seedTelemetry(1)

      await loadDashboard()
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Unable to generate telemetry.',
      )
    } finally {
      setGenerating(false)
    }
  }

  return (
    <div className="live-page">
      <header className="live-header">
        <div>
          <button
            type="button"
            className="live-back"
            onClick={onBack}
          >
            ← Back to Sensors
          </button>

          <p>
            SMART-X / LIVE MONITORING
          </p>

          <h1>
            Real-Time Telemetry Dashboard
          </h1>

          <span>
            Live sensor health,
            anomaly detection and
            telemetry trends.
          </span>
        </div>

        <button
          type="button"
          className="generate-button"
          onClick={generateReading}
          disabled={generating}
        >
          {generating
            ? 'Generating...'
            : 'Generate Live Telemetry'}
        </button>
      </header>

      <main className="live-main">
        {error && (
          <div className="live-error">
            {error}
          </div>
        )}

        {loading ? (
          <p>Loading dashboard...</p>
        ) : dashboard ? (
          <>
            <section className="summary-grid">
              <article>
                <span>
                  Total Sensors
                </span>
                <strong>
                  {dashboard.totalSensors}
                </strong>
              </article>

              <article>
                <span>
                  Connected
                </span>
                <strong>
                  {
                    dashboard.connectedSensors
                  }
                </strong>
              </article>

              <article>
                <span>
                  Anomalies
                </span>
                <strong>
                  {dashboard.anomalyCount}
                </strong>
              </article>
            </section>

            {dashboard.anomalyCount >
              0 && (
              <div className="anomaly-banner">
                ⚠ Telemetry anomalies
                detected. Review highlighted
                sensors below.
              </div>
            )}

            <section className="live-grid">
              {dashboard.sensors.map(
                (sensor) => (
                  <article
                    key={sensor.sensorId}
                    className={`live-card ${
                      sensor.isAnomaly
                        ? 'anomaly-card'
                        : !sensor.isConnected
                          ? 'offline-card'
                          : ''
                    }`}
                  >
                    <div className="live-card-top">
                      <div>
                        <span className="live-category">
                          {
                            sensor.category
                          }
                        </span>

                        <h2>
                          {
                            sensor.deviceIdentifier
                          }
                        </h2>
                      </div>

                      <span
                        className={`live-status ${
                          sensor.isConnected
                            ? 'status-connected'
                            : 'status-disconnected'
                        }`}
                      >
                        <span className="live-dot" />

                        {sensor.isConnected
                          ? 'CONNECTED'
                          : 'DISCONNECTED'}
                      </span>
                    </div>

                    <div className="reading">
                      <span>
                        {sensor.metric}
                      </span>

                      <strong>
                        {
                          sensor.latestValue
                        }
                        {sensor.unit}
                      </strong>
                    </div>

                    <TelemetrySparkline
                      values={
                        sensor.recentValues
                      }
                    />

                    <div
                      className={`sensor-message ${
                        sensor.isAnomaly
                          ? 'message-anomaly'
                          : ''
                      }`}
                    >
                      {
                        sensor.statusMessage
                      }
                    </div>

                    {sensor.latestTimestampUtc && (
                      <small>
                        Last reading:{' '}
                        {new Date(
                          sensor.latestTimestampUtc,
                        ).toLocaleTimeString()}
                      </small>
                    )}
                  </article>
                ),
              )}
            </section>
          </>
        ) : null}
      </main>
    </div>
  )
}

export default LiveTelemetryDashboardPage