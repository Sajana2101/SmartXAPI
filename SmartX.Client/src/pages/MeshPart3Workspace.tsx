
import MeshTopologyPage from './MeshTopologyPage'
import MeshTelemetryHistoryPanel from './MeshTelemetryHistoryPanel'

interface MeshPart3WorkspaceProps {
  onBack: () => void
}

export default function MeshPart3Workspace({
  onBack,
}: MeshPart3WorkspaceProps) {
  return (
    <div className="mesh-page">
      <MeshTopologyPage onBack={onBack} />

      <div className="mesh-shell" style={{ paddingTop: 0 }}>
        <MeshTelemetryHistoryPanel />
      </div>
    </div>
  )
}
