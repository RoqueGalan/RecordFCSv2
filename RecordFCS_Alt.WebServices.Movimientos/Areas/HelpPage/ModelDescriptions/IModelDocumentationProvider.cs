using System;
using System.Reflection;

namespace RecordFCS_Alt.WebServices.Movimientos.Areas.HelpPage.ModelDescriptions
{
    public interface IModelDocumentationProvider
    {
        string GetDocumentation(MemberInfo member);

        string GetDocumentation(Type type);
    }
}