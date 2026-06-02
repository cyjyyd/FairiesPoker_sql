using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FairiesPoker
{
    internal static class AppIcon
    {
        private static readonly HashSet<Form> AppliedForms = new HashSet<Form>();
        private static Icon icon;

        public static void ApplyToOpenForms()
        {
            for (int i = 0; i < Application.OpenForms.Count; i++)
            {
                Apply(Application.OpenForms[i]);
            }
        }

        private static void Apply(Form form)
        {
            if (!AppliedForms.Add(form))
            {
                return;
            }

            Icon appIcon = GetIcon();
            if (appIcon != null)
            {
                form.Icon = (Icon)appIcon.Clone();
            }

            form.FormClosed += (_, _) => AppliedForms.Remove(form);
        }

        private static Icon GetIcon()
        {
            if (icon == null)
            {
                icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }

            return icon;
        }
    }
}
