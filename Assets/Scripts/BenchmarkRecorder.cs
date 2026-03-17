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
        public double fps;
        public double frameTime;
        
        public double cpuFrame;
        public double gpuFrame;

        public long drawCalls;
        public long setPass;
        public long batches;

        public long triangles;
        public long vertices;
        
        public double totalMem;
        public double vram;

        public long shadowCasters;
    }

    List<FrameSample> samples = new List<FrameSample>();

    ProfilerRecorder cpuFrameTime;
    ProfilerRecorder gpuFrameTime;
    ProfilerRecorder drawCalls;
    ProfilerRecorder setPassCalls;
    ProfilerRecorder batches;
    ProfilerRecorder triangles;
    ProfilerRecorder vertices;
    ProfilerRecorder totalMemory;
    ProfilerRecorder vram;
    ProfilerRecorder shadowCasters;

    void Start()
    {
        cpuFrameTime = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "CPU Total Frame Time");
        gpuFrameTime = ProfilerRecorder.StartNew(ProfilerCategory.Render, "GPU Frame Time");
        drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
        setPassCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
        triangles = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
        vertices = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
        totalMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
        vram = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx Used Memory");
        shadowCasters = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Shadow Casters Count");
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;

        if (timer >= warmupDuration && timer <= warmupDuration + benchmarkDuration)
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

            const double nsToMs = 0.000001;
            s.cpuFrame = cpuFrameTime.LastValue * nsToMs;
            s.gpuFrame = gpuFrameTime.LastValue * nsToMs;
            
            s.drawCalls = drawCalls.LastValue;
            s.setPass = setPassCalls.LastValue;
            s.batches = batches.LastValue;
            
            s.triangles = triangles.LastValue;
            s.vertices = vertices.LastValue;

            const double bytesToMB = 1.0 / (1024.0 * 1024.0);
            s.totalMem = totalMemory.LastValue * bytesToMB;
            s.vram = vram.LastValue * bytesToMB;

            s.shadowCasters = shadowCasters.LastValue;
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

        sb.AppendLine("frame,fps,frameTime,cpuFrameTime,gpuFrameTime,drawCalls,setPassCalls,batches,triangles,vertices,totalMemory,vramUsage,shadowCasters");        
        foreach (var s in samples)
        {
            sb.AppendLine(
                s.frame + "," +
                s.fps.ToString(CultureInfo.InvariantCulture) + "," +
                s.frameTime.ToString(CultureInfo.InvariantCulture) + "," +
                s.cpuFrame.ToString(CultureInfo.InvariantCulture) + "," +
                s.gpuFrame.ToString(CultureInfo.InvariantCulture) + "," +
                s.drawCalls + "," +
                s.setPass + "," +
                s.batches + "," +
                s.triangles + "," +
                s.vertices + "," +
                s.totalMem.ToString(CultureInfo.InvariantCulture) + "," +
                s.vram.ToString(CultureInfo.InvariantCulture) + "," +
                s.shadowCasters
            );
        }

        string buildFolder = Directory.GetParent(Application.dataPath).FullName;
        string filename = fileName + " " + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
        string path = Path.Combine(buildFolder, filename);

        File.WriteAllText(path, sb.ToString());
    }
}