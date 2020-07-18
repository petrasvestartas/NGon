using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace NGonGh {
    public class SubDInfo : GH_AssemblyInfo {
        public override string Name => "NGon 2.1";

        public override Bitmap Icon => Properties.Resources.Icons_NGons_FromPolylines;

        public override string Description => "Extension methods for easier NGons manipulation in Meshes";

        public override Guid Id => new Guid("20563e24-568f-4f4f-b61b-71a1781ef92f");

        public override string AuthorName => "Petras Vestartas";

        public override string AuthorContact => "petrasvestartas@gmail.com www.petrasvestartas.com";

        public override Bitmap AssemblyIcon => Properties.Resources.Icons_NGons_FromPolylines;


    }
}
