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