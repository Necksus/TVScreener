using TVScreener.Controllers;

namespace TVScreener.ZoneBourse;

public class Country : JsTreeModel
{
    public Country(string id, string name, string parentId, bool hasChildren)
        : base(id, name, parentId, hasChildren)
    {
    }
}
