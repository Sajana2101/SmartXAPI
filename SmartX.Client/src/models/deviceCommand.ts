
export type DeviceCommandType =
  | 'EnableDevice'
  | 'DisableDevice'
  | 'RestartDevice'
  | 'SetSamplingInterval'
  | 'AcknowledgeAlert'

export type DeviceCommandStatus =
  | 'Queued'
  | 'Executing'
  | 'Completed'
  | 'Failed'
  | 'Undone'

export interface DeviceCommand {
  id: string
  sensorId: string
  type: DeviceCommandType
  status: DeviceCommandStatus
  samplingIntervalSeconds: number | null
  createdAtUtc: string
  executedAtUtc: string | null
  completedAtUtc: string | null
  message: string | null
}

export interface CreateDeviceCommandRequest {
  sensorId: string
  type: DeviceCommandType
  samplingIntervalSeconds?: number | null
}

export interface DeviceOperatingState {
  sensorId: string
  enabled: boolean
  samplingIntervalSeconds: number
}
