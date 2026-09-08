using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class DeploymentValidator
    {
        public bool ContainsNode(
            DeploymentNode current,
            string nodeName)
        {
            if (current.Name.Equals(
                nodeName,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            foreach (DeploymentNode child in current.Children)
            {
                if (ContainsNode(child, nodeName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}