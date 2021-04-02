using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.ResponseModel;
using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.Directives
{
    public class UpdateIndexListDataDirective : IDirective
    {
        public string type => "Alexa.Presentation.APL.UpdateIndexListData";
        public Dictionary<string, IData> datasources { get; set; }
        public string token { get; set; }
        public string listId { get; set; }
        public string listVersion { get; set; }
        public List<IUpdateOperation> operations { get; set; }

    }

    public interface IUpdateOperation
    {

    }

    public class InsertItem : IUpdateOperation
    {
        public string type => nameof(InsertItem);
        public int index { get; set; }
        public object item { get; set; }
    }

    public class InsertMultipleItems : IUpdateOperation
    {
        public string type => nameof(InsertMultipleItems);
        public List<object> items { get; set; }
        public int index { get; set; }
    }

    public class SetItem : IUpdateOperation
    {
        public string type => nameof(SetItem);
        public int index { get; set; }
        public object item { get; set; }
    }

    public class DeleteItem : IUpdateOperation
    {
        public string type => nameof(DeleteItem);
        public int index { get; set; }
    }

    public class DeleteMultipleItems : IUpdateOperation
    {
        public string type => nameof(DeleteMultipleItems);
        public int count { get; set; }
        public int index { get; set; }
    }
}
