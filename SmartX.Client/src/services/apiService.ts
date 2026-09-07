export interface ApiHealth {
  status: string
  application: string
}

const API_BASE_URL = 'https://localhost:7179'

export async function getApiHealth(): Promise<ApiHealth> {
  const response = await fetch(`${API_BASE_URL}/api/health`)

  if (!response.ok) {
    throw new Error('Unable to connect to the Smart-X API')
  }

  return response.json()
}