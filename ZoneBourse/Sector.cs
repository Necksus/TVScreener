using TVScreener.Controllers;

namespace TVScreener.ZoneBourse
{
    public class Sector : JsTreeModel
    {
        public Sector(string id, string name, string parentId, bool hasChildren) 
            : base(id, name, parentId, hasChildren)
        {
        }
    }
}
