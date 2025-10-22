using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViDi2;
using ViDi2.Local;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class DeepInspectionConfig
    {
        public static string WorkspacePath = Machine.config.setup.WorkspacePath;
        public static string WorkspaceName = "DuckYangFinal";
        public static string StreamName = "기본값";
        public static string StreamTool = "Locate";
    }

    public class CognexDeepVision
    {
        // Private static variable to hold the single instance of the class
        private ViDi2.Runtime.Local.Control control;
        private ViDi2.Runtime.IWorkspace workspace;
        private IStream stream;
        // Private constructor to prevent instantiation from outside
        public CognexDeepVision()
        {
            // Initializes the control
            // This initialization does not allocate any gpu ressources.
        
            control = new ViDi2.Runtime.Local.Control(GpuMode.Deferred);

            // Initializes all CUDA devices

            control.InitializeComputeDevices(GpuMode.SingleDevicePerTool, new List<int>() { });

            // Stabilize all Compute devices

            control.StabilizeComputeDevices(StabilizeMode.GPU);

            // Open a runtime workspace from file
            // the path to this file relative to the example root folder
            // and assumes the resource archive was extracted there.

            workspace = control.Workspaces.Add(DeepInspectionConfig.WorkspaceName, DeepInspectionConfig.WorkspacePath);

            // Store a reference to the stream 'default'

            stream = workspace.Streams[DeepInspectionConfig.StreamName];
        }

        // Example method to demonstrate functionality
        public ReadOnlyCollection<IFeature> Infer(string FilePath)
        {
            // Load an image from file
            using (IImage image = new LibraryImage(FilePath)) //disposing the image when we do not need it anymore
            {
                // Allocates a sample with the image
                using (ISample sample = stream.CreateSample(image))
                {
                    ITool blueTool = stream.Tools[DeepInspectionConfig.StreamTool];


                    DateTime dtS = DateTime.Now;

                    // Process the image by the tool. All upstream tools are also processed
                    sample.Process(blueTool);
                    //Console.WriteLine($"Total Time: {(DateTime.Now - dtS).TotalMilliseconds}");
                    //Machine.logger.Write(eLogType.INSPECTION, $"Total Time: {(DateTime.Now - dtS).TotalMilliseconds}");
                    IBlueMarking blueMarking = sample.Markings[blueTool.Name] as IBlueMarking;

                    foreach (IBlueView view in blueMarking.Views)
                    {
                        return view.Features;
                    }
                }
            }
            return new ReadOnlyCollection<IFeature>(new List<IFeature>());
        }


    }
}


