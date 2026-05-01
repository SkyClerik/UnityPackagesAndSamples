using UnityEngine;

namespace SkyClerik.Develop
{
    public interface ISelectionTabService
    {
        string Title { get; }
        string Description { get; }
        Texture2D Icon { get; }
        bool IsAvailable(Object[] selection);
        void Execute(SelectionTabWindow window);
    }
}