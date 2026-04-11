# Optimalizácia výkonu v prostredí Unity
**Autor:** Adam Gál  
**Univerzita:** Univerzita Konštantína Filozofa v Nitre, Fakulta prírodných vied a informatiky  
**Školiteľ:** Mgr. Matúš Valko  
**Rok:** 2026  
**Verzia Unity:** 6000.3.2f1 LTS  
**Render Pipeline:** Universal Render Pipeline (URP)

---

## O projekte

Projekt sa zaoberá optimalizáciou výkonu 3D aplikácií v prostredí Unity. Na tento účel bola navrhnutá testovacia scéna s vysokou geometrickou a výpočtovou zložitosťou, ktorá bola implementovaná vo viacerých variantoch. Každý variant využíva odlišné optimalizačné postupy, pričom ostatné podmienky zostávajú rovnaké, aby bolo možné objektívne porovnanie výsledkov.

---

## Štruktúra vetiev

Každá optimalizačná technika je implementovaná v samostatnej Git vetve odvodenej z referenčnej verzie (`main`):

```
main - referenčná (neoptimalizovaná) scéna
 |-- level-of-detail
 |-- static-batching
 |-- gpu-instancing
 |-- optimalizacia-osvetlenia
 |-- optimalizacia-tienov
 |-- mipmap
 |-- occlusion-culling
 |-- optimalizacie-renderingu - kombinácia: LOD + batching + instancing + culling
 |-- vsetky-optimalizacie - kombinácia všetkých (bez osvetlenia)
```

---

## Meranie výkonu

Projekt obsahuje skripty `BenchmarkCamera` a `BenchmarkRecorder`, ktoré automaticky:
- Pohybujú kamerou po deterministickej trajektórii
- Zaznamenávajú výkonové metriky každý snímok pomocou `ProfilerRecorder`
- Exportujú dáta do `.csv` súboru

### Sledované metriky:
`FPS` · `Frame Time` · `CPU Frame Time` · `GPU Frame Time` · `Draw Calls` · `SetPass Calls` · `Batches` · `Triangles` · `Vertices` · `Total Memory` · `VRAM` · `Shadow Casters`

> Pre meranie VRAM je potrebné nastaviť projekt do režimu **Development Build**.

---

## Použité assety

- [Socha leva](https://assetstore.unity.com/packages/3d/props/exterior/hq-lion-statue-50736)
- [Drevený dom](https://assetstore.unity.com/packages/3d/environments/wooden-house-free-low-poly-270889)
- [Loď](https://assetstore.unity.com/packages/3d/environments/historic/colonial-ship-70472)
- [Kríky a porast](https://assetstore.unity.com/packages/3d/environments/unity-terrain-urp-demo-scene-213197#content)
- [Palmy](https://assetstore.unity.com/packages/3d/vegetation/trees/mobile-palm-pack-urp-277937)
- [Vodná plocha](https://assetstore.unity.com/packages/vfx/shaders/one-click-add-water-stylized-water-shader-305970)
- [Kamene](https://assetstore.unity.com/packages/essentials/tutorial-projects/book-of-the-dead-environment-hdrp-121175)
- [Stromy](https://www.turbosquid.com/3d-models/broadleaf-tree-04-884791)
- [Skybox](https://assetstore.unity.com/packages/2d/textures-materials/sky/skybox-series-free-103633)
