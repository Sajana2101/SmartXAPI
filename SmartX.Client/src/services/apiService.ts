import type {
  DeviceAttachment,
} from '../models/attachment'

import type {
  TelemetryDashboard,
} from '../models/telemetryDashboard'

import type {
  TelemetryPacket,
  TelemetryRequest,
} from '../models/telemetry'


import type {
  Sensor,
  SensorRequest,
} from '../models/sensor'

export interface ApiHealth {
  status: string
  application: string
}

const API_BASE_URL = 'https://localhost:7179'

async function getErrorMessage(
  response: Response,
): Promise<string> {
  try {
    const data = await response.json()

    if (data.message) {
      return data.message
    }

    if (data.title) {
      return data.title
    }
  } catch {
    return 'An unexpected API error occurred.'
  }

  return 'An unexpected API error occurred.'
}

export async function getApiHealth(): Promise<ApiHealth> {
  const response = await fetch(`${API_BASE_URL}/api/health`)

  if (!response.ok) {
    throw new Error('Unable to connect to the Smart-X API')
  }

  return response.json()
}

export async function getSensors(): Promise<Sensor[]> {
  const response = await fetch(`${API_BASE_URL}/api/sensors`)

  if (!response.ok) {
    throw new Error(await getErrorMessage(response))
  }

  return response.json()
}

export async function createSensor(
  sensor: SensorRequest,
): Promise<Sensor> {
  const response = await fetch(`${API_BASE_URL}/api/sensors`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(sensor),
  })

  if (!response.ok) {
    throw new Error(await getErrorMessage(response))
  }

  return response.json()
}

export async function updateSensor(
  id: string,
  sensor: SensorRequest,
): Promise<Sensor> {
  const response = await fetch(
    `${API_BASE_URL}/api/sensors/${id}`,
    {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(sensor),
    },
  )

  if (!response.ok) {
    throw new Error(await getErrorMessage(response))
  }

  return response.json()
}

export async function deleteSensor(
  id: string,
): Promise<void> {
  const response = await fetch(
    `${API_BASE_URL}/api/sensors/${id}`,
    {
      method: 'DELETE',
    },
  )

  if (!response.ok) {
    throw new Error(await getErrorMessage(response))
  }
}

export async function createFloatTelemetry(
  request: TelemetryRequest<number>,
): Promise<TelemetryPacket<number>> {
  const response = await fetch(
    `${API_BASE_URL}/api/telemetry/float`,
    {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    },
  )

  if (!response.ok) {
    throw new Error(await getErrorMessage(response))
  }

  return response.json()
}

export async function createIntTelemetry(
  request: TelemetryRequest<number>,
): Promise<TelemetryPacket<number>> {
  const response = await fetch(
    `${API_BASE_URL}/api/telemetry/int`,
    {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    },
  )

  if (!response.ok) {
    throw new Error(await getErrorMessage(response))
  }

  return response.json()
}

export async function createBoolTelemetry(
  request: TelemetryRequest<boolean>,
): Promise<TelemetryPacket<boolean>> {
  const response = await fetch(
    `${API_BASE_URL}/api/telemetry/bool`,
    {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    },
  )

  if (!response.ok) {
    throw new Error(await getErrorMessage(response))
  }

  return response.json()
}

export async function getFloatTelemetry():
  Promise<TelemetryPacket<number>[]> {
  const response = await fetch(
    `${API_BASE_URL}/api/telemetry/float`,
  )

  if (!response.ok) {
    throw new Error(await getErrorMessage(response))
  }

  return response.json()
}

export async function getIntTelemetry():
  Promise<TelemetryPacket<number>[]> {
  const response = await fetch(
    `${API_BASE_URL}/api/telemetry/int`,
  )

  if (!response.ok) {
    throw new Error(await getErrorMessage(response))
  }

  return response.json()
}

export async function getBoolTelemetry():
  Promise<TelemetryPacket<boolean>[]> {
  const response = await fetch(
    `${API_BASE_URL}/api/telemetry/bool`,
  )

  if (!response.ok) {
    throw new Error(await getErrorMessage(response))
  }

  return response.json()
}
export async function getAttachments(
  sensorId: string,
): Promise<DeviceAttachment[]> {
  const response = await fetch(
    `${API_BASE_URL}/api/attachments/sensor/${sensorId}`,
  )

  if (!response.ok) {
    throw new Error(await getErrorMessage(response))
  }

  return response.json()
}

export async function uploadAttachment(
  sensorId: string,
  file: File,
): Promise<DeviceAttachment> {
  const formData = new FormData()

  formData.append('file', file)

  const response = await fetch(
    `${API_BASE_URL}/api/attachments/sensor/${sensorId}`,
    {
      method: 'POST',
      body: formData,
    },
  )

  if (!response.ok) {
    throw new Error(await getErrorMessage(response))
  }

  return response.json()
}

export async function downloadAttachment(
  attachment: DeviceAttachment,
): Promise<void> {
  const response = await fetch(
    `${API_BASE_URL}/api/attachments/${attachment.id}/download`,
  )

  if (!response.ok) {
    throw new Error(await getErrorMessage(response))
  }

  const blob = await response.blob()

  const url = URL.createObjectURL(blob)

  const anchor = document.createElement('a')

  anchor.href = url
  anchor.download = attachment.fileName

  document.body.appendChild(anchor)

  anchor.click()

  anchor.remove()

  URL.revokeObjectURL(url)
}

export async function deleteAttachment(
  id: string,
): Promise<void> {
  const response = await fetch(
    `${API_BASE_URL}/api/attachments/${id}`,
    {
      method: 'DELETE',
    },
  )

  if (!response.ok) {
    throw new Error(await getErrorMessage(response))
  }
}
export async function getTelemetryDashboard():
  Promise<TelemetryDashboard> {
  const response = await fetch(
    `${API_BASE_URL}/api/telemetry-dashboard`,
  )

  if (!response.ok) {
    throw new Error(
      await getErrorMessage(response),
    )
  }

  return response.json()
}

export async function seedTelemetry(
  readingsPerSensor: number,
): Promise<number> {
  const response = await fetch(
    `${API_BASE_URL}/api/telemetry/seed/${readingsPerSensor}`,
    {
      method: 'POST',
    },
  )

  if (!response.ok) {
    throw new Error(
      await getErrorMessage(response),
    )
  }

  const result =
    await response.json()

  return result.readingsCreated
}