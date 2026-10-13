
export type DeviceIssueType =
  | 'Error'
  | 'Disconnected'

export interface DeviceIssue {
  sensorId: string
  type: DeviceIssueType
  firstDetectedAtUtc: string
}

export interface DeviceIssueResponse {
  activeCount: number
  issues: DeviceIssue[]
}

export interface DeviceIssueOperationResult {
  added?: boolean
  resolved?: boolean
  activeCount: number
}
