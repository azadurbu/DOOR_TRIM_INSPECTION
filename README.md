# DOOR_TRIM_INSPECTION

**Automotive Door Trim Vision Inspection System**

A modern vision-based inspection platform for verifying door trim assembly accuracy in automotive manufacturing.

DOOR_TRIM_INSPECTION is a real-time camera-driven inspection system that automatically checks screws, plugs, speakers, fasteners, pads, and other components installed on a vehicle’s door trim. Built using **C#, WPF, OpenCVSharp, and Cognex vision tools**, the system integrates with **PLC signals** to control the production line, ensuring fast, reliable, and model-specific inspection results—reporting **OK** or **NG** based on precise visual criteria. Designed for high-volume production environments, it improves quality, reduces manual errors, and maintains consistency across all inspected door trims.

---

## 📌 Overview of Real-Time Inspection Process

1. **PLC Signal Input**

   * The system receives a **start signal from the PLC** indicating that a door trim has been placed in the machine by the operator.

2. **Operator Placement**

   * The operator positions the door trim onto the door holder inside the inspection machine.

3. **Lighting & Camera Triggering**

   * Once the door trim is in position, the machine turns on the proper inspection lights.
   * Two cameras capture images of both sides of the door trim.

4. **Image Acquisition & Processing**

   * Captured images are sent to the vision system (OpenCVSharp + Cognex).
   * Pre-processing includes ROI cropping, noise reduction, thresholding, edge detection, and color enhancement.

5. **Component Inspection**

   * Each component is inspected based on the selected model’s recipe:

     * Screws → presence, orientation, proper seating
     * Plugs → handle/control connectors
     * Speaker → presence and shape
     * Fasteners/Clips → presence and alignment
     * Pads / Foam → correct placement
     * Model-specific components

6. **Decision & Output**

   * If all components pass → **OK**
   * If any component fails → **NG**
   * The result is displayed on the monitor in real-time.

7. **PLC Signal Output**

   * The system sends an **OK/NG signal back to the PLC** to control downstream processes (e.g., conveyor movement, sorting, alarms).

8. **Logging**

   * All inspection results, timestamps, model info, and captured images are saved in `/Log/` for traceability.

---

## 📁 Folder Structure (Inside `DOOR_TRIM_INSPECTION/`)

```
DOOR_TRIM_INSPECTION/
│
├── Class/                # Core classes: inspection logic, utilities, data models
├── Controls/             # Custom WPF controls (buttons, UI components)
├── Device/               # Camera & Cognex device integration
├── Form/                 # Additional UI windows, dialogs, operator forms
├── Images/               # Reference/template images, ROI masks, snapshots
├── Log/                  # Inspection logs, NG/OK images, history
├── Properties/           # Project metadata, settings, assembly info
├── Resources/            # UI resources, icons, XAML resources, fonts
├── obj/                  # Intermediate build files (auto-generated)
│
├── App.config            # Application configuration (paths, runtime settings)
├── App.xaml              # WPF application root
├── App.xaml.cs           # Application startup logic
│
├── MainWindow.xaml       # Main UI layout (camera view, inspection result, controls)
├── MainWindow.xaml.cs    # Main application logic and event handling
│
├── DOOR_TRIM_INSPECTION.csproj        # Project configuration
├── DOOR_TRIM_INSPECTION.csproj.user   # VS user-specific config
│
├── app.manifest          # Application manifest (permissions, DPI settings)
└── packages.config       # NuGet package references (OpenCVSharp, Cognex, etc.)
```

---

## 🔍 Core System Features

### Vision-Based Component Checks

* Screws → presence, orientation, proper seating
* Fasteners / Clips → presence, alignment
* Plugs → handle/control connector verification
* Speaker → presence, correct shape
* Pads / Foam → proper positioning
* Multi-model support with per-model ROI recipes

### PLC Integration

* Receives start signal from PLC when a door trim enters the machine
* Sends OK/NG output signals to PLC to control downstream processes

### WPF Application Features

* Live feed of both door trim sides
* Real-time inspection overlay (ROIs, detected components)
* OK/NG indicator on the monitor
* Model selection for inspection
* Parameter adjustment for fine-tuning detection
* Logging of results and captured images

---

## 🎥 Real-Time Inspection Demo

The repository includes:

```
duckyang-doortrim-inspection video.mkv
```

The video demonstrates:

* Operator placing the door trim on the holder
* Machine lights turning on and cameras capturing both sides
* Real-time vision analysis of screws, plugs, speakers, fasteners, pads
* OK/NG results displayed on the monitor
* Integration with PLC signals for automated process control

---

## 🧩 Inspection Workflow Overview

```
PLC sends start signal
        │
        ▼
Operator places door trim on holder
        │
        ▼
Machine lights turn on → Cameras capture left & right sides
        │
        ▼
Vision Engine (OpenCVSharp + Cognex)
   • ROI cropping
   • Pre-processing
   • Component inspection (screws, plugs, speaker, fasteners, pads)
        │
        ▼
Decision Module
   • All components OK → Output: OK
   • Any component missing/faulty → Output: NG
        │
        ▼
Monitor Display & PLC Output
   • Show OK/NG
   • Send OK/NG signal to PLC
   • Save images & results in /Log/
```

---

## 📄 Example Inspection Logic (C#)

```csharp
public InspectionResult Run(Bitmap leftSide, Bitmap rightSide, Recipe recipe)
{
    var result = new InspectionResult();

    result.Screw = ScrewCheck(leftSide, recipe.ScrewRoi) &&
                   ScrewCheck(rightSide, recipe.ScrewRoi);
    result.Plug = PlugCheck(leftSide, recipe.PlugRoi) &&
                  PlugCheck(rightSide, recipe.PlugRoi);
    result.Speaker = SpeakerCheck(leftSide, recipe.SpeakerRoi) &&
                     SpeakerCheck(rightSide, recipe.SpeakerRoi);
    result.Fastener = FastenerCheck(leftSide, recipe.FastenerRoi) &&
                      FastenerCheck(rightSide, recipe.FastenerRoi);
    result.Pad = PadCheck(leftSide, recipe.PadRoi) &&
                 PadCheck(rightSide, recipe.PadRoi);

    result.Overall = result.AllPass() ? "OK" : "NG";

    // Send result to PLC
    PLC.SendSignal(result.Overall);

    // Save inspection logs
    Logger.Save(result, leftSide, rightSide);
    return result;
}
```

---

## 📌 Future Enhancements

* AI-based defect detection (YOLO/ONNX/deep learning)
* Automatic model recognition for hands-free setup
* Multi-camera synchronized inspection
* Advanced PLC integration for complete line automation
* Web dashboard for real-time monitoring and production metrics

---

This README now fully captures:

* **Operator interaction**
* **PLC input/output**
* **Dual-side camera capture**
* **Component analysis**
* **OK/NG decision logic**
* **Logging** and real-time display
