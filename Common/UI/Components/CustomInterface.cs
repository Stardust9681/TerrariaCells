using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariaCells.Common.UI.Components
{
    public abstract class CustomUserInterface : Terraria.UI.UserInterface
    {
        public new Windows.WindowState? CurrentState => (Windows.WindowState)base.CurrentState;
    }
    public class CustomInterface<T> : CustomUserInterface where T : Windows.WindowState
    {
        public void SetState(T state)
        {
            base.SetState(state);
        }
        public new void SetState(Terraria.UI.UIState state)
        {
            if (state.GetType() is not T)
            {
                throw new ArgumentException($"UI State of wrong type was given. Received: {state.GetType().FullName} Expected: {typeof(T).FullName}", nameof(state));
            }
            base.SetState(state);
        }
    }
}