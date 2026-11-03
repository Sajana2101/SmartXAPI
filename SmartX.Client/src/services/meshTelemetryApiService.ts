
import type { HistoricalTelemetryEntry } from '../models/telemetryHistory'

const API_BASE_URL = 'https://localhost:7179'

export async function getMeshTelemetryRange(
  sensorId: string,
  fromUtc: string,
  toUtc: string,
  limit = 200,
): Promise<HistoricalTelemetryEntry[]> {
  const parameters = new URLSearchParams({
    sensorId,
    fromUtc,
    toUtc,
    limit: String(limit),
  })

  const response = await fetch(
    `${API_BASE_URL}/api/telemetry-history/range?${parameters.toString()}`,
  )

  if (!response.ok) {
    let message = `Telemetry query failed (${response.status}).`

    try {
      const result: unknown = await response.json()

      if (
        typeof result === 'object' &&
        result !== null &&
        'message' in result &&
        typeof result.message === 'string'
      ) {
        message = result.message
      }
    } catch {
      message = `Telemetry query failed (${response.status}).`
    }

    throw new Error(message)
  }

  return (await response.json()) as HistoricalTelemetryEntry[]
}
