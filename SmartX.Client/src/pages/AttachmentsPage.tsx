import {
  useEffect,
  useState,
} from 'react'

import type {
  Sensor,
} from '../models/sensor'

import type {
  DeviceAttachment,
} from '../models/attachment'

import {
  deleteAttachment,
  downloadAttachment,
  getAttachments,
  getSensors,
  uploadAttachment,
} from '../services/apiService'

import './AttachmentsPage.css'

interface AttachmentsPageProps {
  onBack: () => void
}

function AttachmentsPage({
  onBack,
}: AttachmentsPageProps) {
  const [sensors, setSensors] =
    useState<Sensor[]>([])

  const [sensorId, setSensorId] =
    useState('')

  const [attachments, setAttachments] =
    useState<DeviceAttachment[]>([])

  const [selectedFile, setSelectedFile] =
    useState<File | null>(null)

  const [loading, setLoading] =
    useState(true)

  const [uploading, setUploading] =
    useState(false)

  const [error, setError] =
    useState('')

  const [success, setSuccess] =
    useState('')

  async function loadAttachments(
    selectedSensorId: string,
  ) {
    if (!selectedSensorId) {
      setAttachments([])
      return
    }

    try {
      const data =
        await getAttachments(
          selectedSensorId,
        )

      setAttachments(data)
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Unable to load attachments.',
      )
    }
  }

  useEffect(() => {
    async function loadSensors() {
      try {
        const data =
          await getSensors()

        setSensors(data)

        if (data.length > 0) {
          setSensorId(data[0].id)

          await loadAttachments(
            data[0].id,
          )
        }
      } catch {
        setError(
          'Unable to load registered sensors.',
        )
      } finally {
        setLoading(false)
      }
    }

    loadSensors()
  }, [])

  async function handleSensorChange(
    id: string,
  ) {
    setSensorId(id)

    setError('')
    setSuccess('')

    await loadAttachments(id)
  }

  async function handleUpload(
    event:
      React.FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    if (!sensorId) {
      setError(
        'Select a sensor first.',
      )
      return
    }

    if (!selectedFile) {
      setError(
        'Choose a file to upload.',
      )
      return
    }

    try {
      setUploading(true)
      setError('')
      setSuccess('')

      await uploadAttachment(
        sensorId,
        selectedFile,
      )

      setSelectedFile(null)

      setSuccess(
        'Attachment uploaded and encrypted successfully.',
      )

      await loadAttachments(
        sensorId,
      )

      const fileInput =
        document.getElementById(
          'attachment-file',
        ) as HTMLInputElement | null

      if (fileInput) {
        fileInput.value = ''
      }
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Unable to upload attachment.',
      )
    } finally {
      setUploading(false)
    }
  }

  async function handleDelete(
    attachment: DeviceAttachment,
  ) {
    const confirmed =
      window.confirm(
        `Delete "${attachment.fileName}"?`,
      )

    if (!confirmed) {
      return
    }

    try {
      await deleteAttachment(
        attachment.id,
      )

      setSuccess(
        'Attachment deleted successfully.',
      )

      await loadAttachments(
        sensorId,
      )
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Unable to delete attachment.',
      )
    }
  }

  function formatFileSize(
    bytes: number,
  ) {
    if (bytes < 1024) {
      return `${bytes} B`
    }

    if (bytes < 1024 * 1024) {
      return `${(
        bytes / 1024
      ).toFixed(1)} KB`
    }

    return `${(
      bytes /
      (1024 * 1024)
    ).toFixed(1)} MB`
  }

  return (
    <div className="attachments-page">
      <header className="attachments-header">
        <button
          type="button"
          className="attachments-back"
          onClick={onBack}
        >
          ← Back to Sensors
        </button>

        <p>SMART-X / DEVICE FILES</p>

        <h1>
          Sensor Attachments
        </h1>

        <span>
          Upload configuration files,
          deployment photographs and
          hardware logs securely.
        </span>
      </header>

      <main className="attachments-main">
        <section className="attachments-panel">
          <h2>
            Upload Attachment
          </h2>

          {error && (
            <div className="attachment-error">
              {error}
            </div>
          )}

          {success && (
            <div className="attachment-success">
              {success}
            </div>
          )}

          {loading ? (
            <p>Loading sensors...</p>
          ) : sensors.length === 0 ? (
            <p>
              Register a sensor before
              uploading attachments.
            </p>
          ) : (
            <form
              onSubmit={handleUpload}
            >
              <label htmlFor="attachment-sensor">
                Sensor
              </label>

              <select
                id="attachment-sensor"
                value={sensorId}
                onChange={(event) =>
                  handleSensorChange(
                    event.target.value,
                  )
                }
              >
                {sensors.map(
                  (sensor) => (
                    <option
                      key={sensor.id}
                      value={sensor.id}
                    >
                      {
                        sensor.deviceIdentifier
                      }
                    </option>
                  ),
                )}
              </select>

              <label htmlFor="attachment-file">
                File
              </label>

              <input
                id="attachment-file"
                type="file"
                accept=".txt,.log,.json,.xml,.cfg,.conf,.jpg,.jpeg,.png,.pdf"
                onChange={(event) =>
                  setSelectedFile(
                    event.target.files?.[0] ??
                      null,
                  )
                }
              />

              <p className="file-help">
                Maximum size: 10 MB.
                Supported: configuration
                files, logs, images and PDF.
              </p>

              <button
                type="submit"
                disabled={uploading}
              >
                {uploading
                  ? 'Encrypting & Uploading...'
                  : 'Upload & Encrypt'}
              </button>
            </form>
          )}
        </section>

        <section className="attachments-panel">
          <div className="attachment-list-heading">
            <h2>
              Sensor Files
            </h2>

            <span>
              {attachments.length}
            </span>
          </div>

          {attachments.length === 0 ? (
            <div className="attachment-empty">
              No attachments for this
              sensor.
            </div>
          ) : (
            <div className="attachment-list">
              {attachments.map(
                (attachment) => (
                  <article
                    key={attachment.id}
                    className="attachment-card"
                  >
                    <div>
                      <h3>
                        {
                          attachment.fileName
                        }
                      </h3>

                      <p>
                        {formatFileSize(
                          attachment.fileSizeBytes,
                        )}
                      </p>

                      {attachment.encryptedAtRest && (
                        <span className="encrypted-badge">
                          AES-256 ENCRYPTED
                        </span>
                      )}
                    </div>

                    <div className="attachment-actions">
                      <button
                        type="button"
                        onClick={() =>
                          downloadAttachment(
                            attachment,
                          )
                        }
                      >
                        Download
                      </button>

                      <button
                        type="button"
                        className="attachment-delete"
                        onClick={() =>
                          handleDelete(
                            attachment,
                          )
                        }
                      >
                        Delete
                      </button>
                    </div>
                  </article>
                ),
              )}
            </div>
          )}
        </section>
      </main>
    </div>
  )
}

export default AttachmentsPage