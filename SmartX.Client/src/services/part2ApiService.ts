
import type {
  CreateDeviceCommandRequest,
  DeviceCommand,
  DeviceOperatingState,
} from '../models/deviceCommand'

import type {
  DeviceAlert,
  DeviceAlertStatus,
  EnqueueAlertRequest,
} from '../models/deviceAlert'

import type {
  DeviceIssueOperationResult,
  DeviceIssueResponse,
  DeviceIssueType,
} from '../models/deviceIssue'

import type {
  HistoricalTelemetryEntry,
  TelemetryHistoryCount,
  TelemetryQueueStatus,
} from '../models/telemetryHistory'

const API_BASE_URL = 'https://localhost:7179'

async function getErrorMessage(
  response: Response,
): Promise<string> {
  try {
    const data: unknown = await response.json()

    if (typeof data === 'object' && data !== null) {
      const result = data as {
        message?: string
        title?: string
        errors?: Record<string, string[]>
      }

      if (result.message) {
        return result.message
      }

      if (result.errors) {
        return Object.entries(result.errors)
          .flatMap(([field, messages]) =>
            messages.map(message => `${field}: ${message}`),
          )
          .join('; ')
      }

      if (result.title) {
        return result.title
      }
    }
  } catch {
    return `API request failed with status ${response.status}.`
  }

  return `API request failed with status ${response.status}.`
}

async function apiRequest<T>(
  endpoint: string,
  options: RequestInit = {},
): Promise<T> {
  const response = await fetch(
    `${API_BASE_URL}${endpoint}`,
    {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...options.headers,
      },
    },
  )

  if (!response.ok) {
    throw new Error(await getErrorMessage(response))
  }

  if (response.status === 204) {
    return null as T
  }

  return response.json() as Promise<T>
}

export async function submitDeviceCommand(
  request: CreateDeviceCommandRequest,
): Promise<DeviceCommand> {
  return apiRequest<DeviceCommand>(
    '/api/device-commands',
    {
      method: 'POST',
      body: JSON.stringify(request),
    },
  )
}

export async function getDeviceCommandHistory():
  Promise<DeviceCommand[]> {
  return apiRequest<DeviceCommand[]>(
    '/api/device-commands',
  )
}

export async function getDeviceCommand(
  id: string,
): Promise<DeviceCommand> {
  return apiRequest<DeviceCommand>(
    `/api/device-commands/${encodeURIComponent(id)}`,
  )
}

export async function undoLastDeviceCommand():
  Promise<DeviceCommand> {
  return apiRequest<DeviceCommand>(
    '/api/device-commands/undo',
    {
      method: 'POST',
    },
  )
}

export async function getDeviceOperatingState(
  sensorId: string,
): Promise<DeviceOperatingState> {
  return apiRequest<DeviceOperatingState>(
    `/api/device-commands/state/${encodeURIComponent(sensorId)}`,
  )
}

export async function enqueueDeviceAlert(
  request: EnqueueAlertRequest,
): Promise<{
  id: string
  severity: string
  pendingCount: number
}> {
  return apiRequest(
    '/api/device-alerts',
    {
      method: 'POST',
      body: JSON.stringify(request),
    },
  )
}

export async function processNextAlert():
  Promise<DeviceAlert | null> {
  return apiRequest<DeviceAlert | null>(
    '/api/device-alerts/process-next',
    {
      method: 'POST',
    },
  )
}

export async function getDeviceAlertStatus():
  Promise<DeviceAlertStatus> {
  return apiRequest<DeviceAlertStatus>(
    '/api/device-alerts/status',
  )
}

export async function getActiveDeviceIssues():
  Promise<DeviceIssueResponse> {
  return apiRequest<DeviceIssueResponse>(
    '/api/device-issues',
  )
}

export async function reportDeviceIssue(
  sensorId: string,
  type: DeviceIssueType,
): Promise<DeviceIssueOperationResult> {
  return apiRequest<DeviceIssueOperationResult>(
    `/api/device-issues/${encodeURIComponent(sensorId)}/${type}`,
    {
      method: 'POST',
    },
  )
}

export async function resolveDeviceIssue(
  sensorId: string,
  type: DeviceIssueType,
): Promise<DeviceIssueOperationResult> {
  return apiRequest<DeviceIssueOperationResult>(
    `/api/device-issues/${encodeURIComponent(sensorId)}/${type}`,
    {
      method: 'DELETE',
    },
  )
}

export async function getSortedTelemetryHistory(
  sensorId?: string,
): Promise<HistoricalTelemetryEntry[]> {
  const query = sensorId
    ? `?sensorId=${encodeURIComponent(sensorId)}`
    : ''

  return apiRequest<HistoricalTelemetryEntry[]>(
    `/api/telemetry-history${query}`,
  )
}

export async function getTelemetryHistoryCount():
  Promise<TelemetryHistoryCount> {
  return apiRequest<TelemetryHistoryCount>(
    '/api/telemetry-history/count',
  )
}

export async function getTelemetryQueueStatus():
  Promise<TelemetryQueueStatus> {
  return apiRequest<TelemetryQueueStatus>(
    '/api/telemetry-queue/status',
  )
}

export async function processTelemetryQueue():
  Promise<{
    processed: number
    remaining: number
  }> {
  return apiRequest(
    '/api/telemetry-queue/process',
    {
      method: 'POST',
    },
  )
}
