# DOOR_TRIM_INSPECTION

**Automotive Door Trim Vision Inspection System – for Dydeokyang Co., Ltd.**

A modern vision-based inspection platform developed for **Dydeokyang Co., Ltd.**, a manufacturer of car door assemblies for Hyundai. This system verifies door trim assembly accuracy in real-time, automatically checking screws, plugs, speakers, fasteners, pads, and other components. Built using **C#, WPF, OpenCVSharp, and Cognex vision tools**, it integrates with **PLC signals**, stores inspection results in **MSSQL**, saves captured images to the file system, and logs device activity. The system ensures fast, reliable, and model-specific inspection results—reporting **OK** or **NG** based on precise visual criteria—helping Dydeokyang maintain high-quality standards in automotive door production.

---

## 📌 Real-Time Inspection Process

The system follows this workflow:

1. **Operator Placement**

   * The operator positions a door trim on the machine’s door holder.

2. **PLC Start Signal**

   * PLC sends a **start signal** indicating the door trim is ready for inspection.

3. **Lighting & Camera Capture**

   * Machine lights turn on.
   * Two cameras capture both left and right sides of the door trim simultaneously.

4. **Vision Engine Processing**

   * Pre-processing: ROI cropping (per door model), noise reduction, thresholding, edge detection, and color enhancement.
   * Component inspection: screws, plugs, speakers, fasteners, pads, and other model-specific components.

5. **Decision Module**

   * If all components pass → **OK**
   * If any component fails → **NG**

6. **Monitor Display & PLC Output**

   * OK/NG result is displayed on the monitor.
   * OK/NG signal is sent back to the PLC for downstream process control.

7. **Data Storage & Logging**

   * Captured images are saved in the **file system**.
   * Inspection results (OK/NG, component status, model, timestamp) are stored in **MSSQL**.
   * Device logs (camera, PLC communication, system events) are stored in `/Log/`.

8. **Next Cycle**

   * Operator places the next door trim on the holder, and the process repeats.

---

## 🔍 Core System Features

### Vision-Based Component Checks

* Screws → presence, orientation, proper seating
* Fasteners / Clips → presence, alignment
* Plugs → handle/control connector verification
* Speaker → presence, correct shape
* Pads / Foam → proper positioning
* Multi-model support with **ROI per door model**

### PLC Integration

* Receives start signal from PLC when door trim enters the machine
* Sends OK/NG output signals to PLC to control downstream processes

### WPF Application Features

* Live feed of both door trim sides
* Real-time inspection overlay (ROIs, detected components)
* OK/NG indicator on the monitor
* Model selection for inspection
* Parameter adjustment for fine-tuning detection

### Data Storage & Logging

* **Images** → stored in file system (raw and processed)
* **Inspection results** → stored in MSSQL (OK/NG, component status, model, timestamps)
* **Device logs** → saved in `/Log/` (camera events, PLC communication, system logs)

---

## 🎥 Real-Time Inspection Demo

The repository includes:

```
duckyang-doortrim-inspection video.mkv
```

The video demonstrates:

* Operator placing the door trim on the holder
* Machine lights turning on and cameras capturing both sides
* Real-time analysis of screws, plugs, speakers, fasteners, pads
* OK/NG results displayed on the monitor
* Integration with PLC signals for automated process control

---

## 🧩 Inspection Workflow Overview

```
Operator places door trim on holder
        │
        ▼
PLC sends start signal
        │
        ▼
Machine lights turn on → Cameras capture left & right sides
        │
        ▼
Vision Engine (OpenCVSharp + Cognex)
   • ROI cropping (per model)
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
   • Save images & results in folder, MSSQL DB, and Log
        │
        ▼
Operator places next door trim on holder
```
---
## Main Software screen Shot
!['Piano screenshot'](https://github.com/azadurbu/DOOR_TRIM_INSPECTION/blob/main/Screenshot%202025-09-18%20112350.jpg) 


## Dydeokyang door trim inspection in action
[![Watch the video](https://github.com/azadurbu/DOOR_TRIM_INSPECTION/blob/main/Screenshot%202025-12-02%20162233.png)]([https://youtu.be/VIDEO_ID](https://www.youtube.com/watch?v=cMi_vfp_FNA))

