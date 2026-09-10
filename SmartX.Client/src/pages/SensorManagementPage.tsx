import { useEffect, useState } from 'react'
import type {
  Sensor,
  SensorCategory,
  SensorRequest,
} from '../models/sensor'
import {
  createSensor,
  deleteSensor,
  getSensors,
  updateSensor,
} from '../services/apiService'
import './SensorManagementPage.css'

interface SensorManagementPageProps {
  onBack: () => void
  onOpenTelemetry: () => void
  onOpenAttachments: () => void
  onOpenLiveDashboard: () => void
}

const emptyForm: SensorRequest = {
  deviceIdentifier: '',
  deploymentLocation: '',
  room: '',
  zone: '',
  nodeId: '',
  category: 'Environmental',
}

function SensorManagementPage({
  onBack,
onOpenTelemetry, 
onOpenAttachments,
  onOpenLiveDashboard,
}: SensorManagementPageProps) {
  const [sensors, setSensors] = useState<Sensor[]>([])
  const [form, setForm] = useState<SensorRequest>(emptyForm)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  async function loadSensors() {
    try {
      setLoading(true)
      setError('')

      const data = await getSensors()

      setSensors(data)
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Unable to load sensors.',
      )
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadSensors()
  }, [])

  function updateField(
    field: keyof SensorRequest,
    value: string,
  ) {
    setForm((current) => ({
      ...current,
      [field]: value,
    }))
  }

  function validateForm(): string | null {
    if (form.deviceIdentifier.trim().length < 3) {
      return 'Enter a valid MAC address or unique device identifier.'
    }

    if (!form.deploymentLocation.trim()) {
      return 'Deployment location is required.'
    }

    if (!form.room.trim()) {
      return 'Room is required.'
    }

    if (!form.zone.trim()) {
      return 'Zone is required.'
    }

    if (!form.nodeId.trim()) {
      return 'Node ID is required.'
    }

    return null
  }

  async function handleSubmit(
    event: React.FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    const validationError = validateForm()

    if (validationError) {
      setError(validationError)
      setSuccess('')
      return
    }

    try {
      setSaving(true)
      setError('')
      setSuccess('')

      if (editingId) {
        await updateSensor(editingId, form)
        setSuccess('Sensor updated successfully.')
      } else {
        await createSensor(form)
        setSuccess('Sensor registered successfully.')
      }

      setForm(emptyForm)
      setEditingId(null)

      await loadSensors()
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Unable to save sensor.',
      )
    } finally {
      setSaving(false)
    }
  }

  function handleEdit(sensor: Sensor) {
    setEditingId(sensor.id)

    setForm({
      deviceIdentifier: sensor.deviceIdentifier,
      deploymentLocation: sensor.deploymentLocation,
      room: sensor.room,
      zone: sensor.zone,
      nodeId: sensor.nodeId,
      category: sensor.category,
    })

    setError('')
    setSuccess('')

    window.scrollTo({
      top: 0,
      behavior: 'smooth',
    })
  }

  function cancelEdit() {
    setEditingId(null)
    setForm(emptyForm)
    setError('')
  }

  async function handleDelete(sensor: Sensor) {
    const confirmed = window.confirm(
      `Delete sensor "${sensor.deviceIdentifier}"?`,
    )

    if (!confirmed) {
      return
    }

    try {
      setError('')
      setSuccess('')

      await deleteSensor(sensor.id)

      setSuccess('Sensor deleted successfully.')

      if (editingId === sensor.id) {
        cancelEdit()
      }

      await loadSensors()
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Unable to delete sensor.',
      )
    }
  }

  function getCategoryLabel(category: SensorCategory) {
    if (category === 'PowerConsumption') {
      return 'Power Consumption'
    }

    return category
  }

  return (
    <div className="sensor-page">
      <header className="sensor-header">
        <div>
          <button
            className="back-button"
            type="button"
            onClick={onBack}
          >
            ← Back to Gateway
          </button>

          <p className="sensor-eyebrow">SMART-X / PART 1</p>

          <h1>Sensor Data Ingestion and Telemetry</h1>

          <p>
            Register and manage devices connected to the Smart-X
            gateway.
          </p>

          <div className="sensor-header-actions">
  <button
    type="button"
    className="save-button"
    onClick={onOpenTelemetry}
  >
    Open Telemetry Ingestion
  </button>

  <button
    type="button"
    className="save-button"
    onClick={onOpenAttachments}
  >
    Manage Attachments
  </button>

  <button
    type="button"
    className="save-button"
    onClick={onOpenLiveDashboard}
  >
    Open Live Dashboard
  </button>
</div>

          <button
  type="button"
  className="save-button"
  onClick={onOpenTelemetry}
>
  Open Telemetry Ingestion
</button>
        </div>
      </header>

      <main className="sensor-main">
        <section className="registration-panel">
          <div className="section-heading">
            <div>
              <p className="section-number">01</p>

              <h2>
                {editingId
                  ? 'Edit Sensor'
                  : 'Register Sensor'}
              </h2>
            </div>

            {editingId && (
              <span className="editing-badge">
                EDITING
              </span>
            )}
          </div>

          {error && (
            <div className="message error-message">
              {error}
            </div>
          )}

          {success && (
            <div className="message success-message">
              {success}
            </div>
          )}

          <form
            className="sensor-form"
            onSubmit={handleSubmit}
          >
            <div className="form-group full-width">
              <label htmlFor="deviceIdentifier">
                MAC Address / Unique Identifier
              </label>

              <input
                id="deviceIdentifier"
                type="text"
                value={form.deviceIdentifier}
                onChange={(event) =>
                  updateField(
                    'deviceIdentifier',
                    event.target.value,
                  )
                }
                placeholder="e.g. AA:BB:CC:DD:EE:01"
                maxLength={100}
              />
            </div>

            <div className="form-group full-width">
              <label htmlFor="deploymentLocation">
                Deployment Location
              </label>

              <input
                id="deploymentLocation"
                type="text"
                value={form.deploymentLocation}
                onChange={(event) =>
                  updateField(
                    'deploymentLocation',
                    event.target.value,
                  )
                }
                placeholder="e.g. Hydroponic Farm A"
                maxLength={150}
              />
            </div>

            <div className="form-group">
              <label htmlFor="room">
                Room
              </label>

              <input
                id="room"
                type="text"
                value={form.room}
                onChange={(event) =>
                  updateField('room', event.target.value)
                }
                placeholder="e.g. Grow Room 2"
                maxLength={100}
              />
            </div>

            <div className="form-group">
              <label htmlFor="zone">
                Zone
              </label>

              <input
                id="zone"
                type="text"
                value={form.zone}
                onChange={(event) =>
                  updateField('zone', event.target.value)
                }
                placeholder="e.g. Zone B"
                maxLength={100}
              />
            </div>

            <div className="form-group">
              <label htmlFor="nodeId">
                Node ID
              </label>

              <input
                id="nodeId"
                type="text"
                value={form.nodeId}
                onChange={(event) =>
                  updateField('nodeId', event.target.value)
                }
                placeholder="e.g. NODE-002"
                maxLength={100}
              />
            </div>

            <div className="form-group">
              <label htmlFor="category">
                Sensor Category
              </label>

              <select
                id="category"
                value={form.category}
                onChange={(event) =>
                  updateField(
                    'category',
                    event.target.value,
                  )
                }
              >
                <option value="Environmental">
                  Environmental
                </option>

                <option value="PowerConsumption">
                  Power Consumption
                </option>

                <option value="Actuator">
                  Actuator
                </option>
              </select>
            </div>

            <div className="form-actions full-width">
              <button
                className="save-button"
                type="submit"
                disabled={saving}
              >
                {saving
                  ? 'Saving...'
                  : editingId
                    ? 'Update Sensor'
                    : 'Register Sensor'}
              </button>

              {editingId && (
                <button
                  className="cancel-button"
                  type="button"
                  onClick={cancelEdit}
                >
                  Cancel Edit
                </button>
              )}
            </div>
          </form>
        </section>

        <section className="registered-panel">
          <div className="registered-heading">
            <div>
              <p className="section-number">02</p>

              <h2>Registered Sensors</h2>
            </div>

            <span className="sensor-count">
              {sensors.length} sensor
              {sensors.length === 1 ? '' : 's'}
            </span>
          </div>

          {loading ? (
            <div className="empty-state">
              Loading sensors...
            </div>
          ) : sensors.length === 0 ? (
            <div className="empty-state">
              <h3>No sensors registered</h3>

              <p>
                Register your first Smart-X sensor using
                the form above.
              </p>
            </div>
          ) : (
            <div className="sensor-list">
              {sensors.map((sensor) => (
                <article
                  className="sensor-card"
                  key={sensor.id}
                >
                  <div className="sensor-card-top">
                    <div>
                      <span className="category-badge">
                        {getCategoryLabel(sensor.category)}
                      </span>

                      <h3>
                        {sensor.deviceIdentifier}
                      </h3>
                    </div>

                    <div className="sensor-actions">
                      <button
                        type="button"
                        onClick={() => handleEdit(sensor)}
                      >
                        Edit
                      </button>

                      <button
                        className="delete-button"
                        type="button"
                        onClick={() =>
                          handleDelete(sensor)
                        }
                      >
                        Delete
                      </button>
                    </div>
                  </div>

                  <dl className="sensor-details">
                    <div>
                      <dt>Location</dt>
                      <dd>
                        {sensor.deploymentLocation}
                      </dd>
                    </div>

                    <div>
                      <dt>Room</dt>
                      <dd>{sensor.room}</dd>
                    </div>

                    <div>
                      <dt>Zone</dt>
                      <dd>{sensor.zone}</dd>
                    </div>

                    <div>
                      <dt>Node ID</dt>
                      <dd>{sensor.nodeId}</dd>
                    </div>
                  </dl>
                </article>
              ))}
            </div>
          )}
        </section>
      </main>
    </div>
  )
}

export default SensorManagementPage