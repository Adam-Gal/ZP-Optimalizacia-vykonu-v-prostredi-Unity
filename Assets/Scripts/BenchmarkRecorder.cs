using UnityEngine;
using Unity.Profiling;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;

public class BenchmarkRecorder : MonoBehaviour
{
    [SerializeField] private string fileName;
    
    const float warmupDuration = 5f;
    const float benchmarkDuration = 60f;

    float timer = 0f;
    int benchmarkFrame = 0;
    bool recording = false;
    bool exported = false;

    struct FrameSample
    {
        public int frame;
        public double frameTime;
        public double fps;

        public double cpuFrame;
        public double mainThread;
        public double gpuFrame;

        public long drawCalls;
        public long batches;
        public long setPass;

        public long triangles;

        public double gcAlloc;
        public double totalMem;
        public double vram;

        public long shadowCasters;
        public long savedByBatching;
    }

    List<FrameSample> samples = new List<FrameSample>();

    ProfilerRecorder cpuFrameTime;
    ProfilerRecorder mainThreadTime;
    ProfilerRecorder gpuFrameTime;
    ProfilerRecorder drawCalls;
    ProfilerRecorder batches;
    ProfilerRecorder setPassCalls;
    ProfilerRecorder triangles;
    ProfilerRecorder gcAlloc;
    ProfilerRecorder totalMemory;
    ProfilerRecorder vram;
    ProfilerRecorder shadowCasters;
    ProfilerRecorder savedByBatching;

    void Start()
    {
        cpuFrameTime = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "CPU Frame Time");
        mainThreadTime = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread");
        gpuFrameTime = ProfilerRecorder.StartNew(ProfilerCategory.Render, "GPU Frame Time");
        drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
        batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
        setPassCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        triangles = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
        gcAlloc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        totalMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
        vram = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx Used Memory");
        shadowCasters = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Shadow Casters Count");
        savedByBatching = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Saved By Batching");
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;

        if (timer > warmupDuration && timer < warmupDuration + benchmarkDuration)
        {
            recording = true;
        }
        else
        {
            recording = false;
        }

        if (recording)
        {
            FrameSample s = new FrameSample();

            s.frame = benchmarkFrame;
            benchmarkFrame++;

            double frameTimeMs = Time.unscaledDeltaTime * 1000.0;
            s.frameTime = frameTimeMs;
            s.fps = 1000.0 / frameTimeMs;

            const double nsToMs = 1e-6;
            s.cpuFrame = cpuFrameTime.LastValue * nsToMs;
            s.mainThread = mainThreadTime.LastValue * nsToMs;
            s.gpuFrame = gpuFrameTime.LastValue * nsToMs;
            
            s.drawCalls = drawCalls.LastValue;
            s.batches = batches.LastValue;
            s.setPass = setPassCalls.LastValue;
            s.triangles = triangles.LastValue;

            const double bytesToMB = 1.0 / (1024.0 * 1024.0);
            s.gcAlloc = gcAlloc.LastValue * bytesToMB;
            s.totalMem = totalMemory.LastValue * bytesToMB;
            s.vram = vram.LastValue * bytesToMB;

            s.shadowCasters = shadowCasters.LastValue;
            s.savedByBatching = savedByBatching.LastValue;
            samples.Add(s);
        }

        if (timer > warmupDuration + benchmarkDuration && !exported)
        {
            ExportCSV();
            exported = true;
        }
    }

    void ExportCSV()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("frame,frameTime,fps,cpuFrameTime,mainThreadTime,gpuFrameTime,drawCalls,batches,setPassCalls,triangles,gcAlloc,totalMemory,vramUsage,shadowCasters,savedByBatching");        foreach (var s in samples)
        {
            sb.AppendLine(
                s.frame + "," +
                s.frameTime.ToString(CultureInfo.InvariantCulture) + "," +
                s.fps.ToString(CultureInfo.InvariantCulture) + "," +
                s.cpuFrame.ToString(CultureInfo.InvariantCulture) + "," +
                s.mainThread.ToString(CultureInfo.InvariantCulture) + "," +
                s.gpuFrame.ToString(CultureInfo.InvariantCulture) + "," +
                s.drawCalls + "," +
                s.batches + "," +
                s.setPass + "," +
                s.triangles + "," +
                s.gcAlloc.ToString(CultureInfo.InvariantCulture) + "," +
                s.totalMem.ToString(CultureInfo.InvariantCulture) + "," +
                s.vram.ToString(CultureInfo.InvariantCulture) + "," +
                s.shadowCasters + "," +
                s.savedByBatching
            );
        }

        string buildFolder = Directory.GetParent(Application.dataPath).FullName;
        string filename = fileName + " " + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
        string path = Path.Combine(buildFolder, filename);

        File.WriteAllText(path, sb.ToString());
    }
}