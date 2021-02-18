using System.Collections.Generic;
using UnityEngine;

public class HexaoidGenerator : MonoBehaviour
{
    // ### Map grid
    int mapBorderX = 0;
    int mapBorderZ = 0;
    [Range(5, 45)]
    int mapBorderPercentage = 40;
    int chunkSizeMin = 0;
    int chunkSizeMax = 0;
    float chunkSizeMinParts = 8f;
    float chunkSizeMaxParts = 4f;
    int underWaterCells = 0;
    int minLandCells = 0;
    int maxLandCells = 0;
    [Range(1, 5)]
    int waterLevel = 3;
    [Range(-40, 0)]
    int elevationMinimum = 0;
    [Range(6, 100)]
    int elevationMaximum = 10;
    int elevationHole = -40;
    int maxHoleCellSize = 100;
    public HexGrid grid;
    struct MapRegion {
        public int xMin, xMax, zMin, zMax;
    }
    MapRegion region;

    // ### Map generation vars
    bool useFixedSeed = true;
    int seed;
    [Range(5, 95)]
    int landPercentage = 50;
    [Range(0f, 1f)]
    float highRiseProbability = 0.25f;
    int highRiseElevation = 3;
    int highSinkElevation = 2;
    [Range(0f, 0.5f)]
    float jitterProbability = 0.25f;
    HexCellPriorityQueue searchFrontier;
    int searchFrontierPhase;
    int cellCount;

    // ### Climate vars
    struct ClimateData {
        public float clouds, moisture;
    }
    List<ClimateData> climate = new List<ClimateData>();
    List<ClimateData> nextClimate = new List<ClimateData>();
    struct Biome {
        public int terrain, warmPlant, temperatePlant, polarPlant;

        public Biome (int terrain, int warmPlant, int temperatePlant, int polarPlant) {
            this.terrain = terrain;
            this.warmPlant = warmPlant;
            this.temperatePlant = temperatePlant;
            this.polarPlant = polarPlant;
        }
    }
    static Biome[] biomes = {
        new Biome(0, 0, 0, 0), new Biome(4, 0, 0, 1), new Biome(4, 0, 0, 2), new Biome(4, 0, 0, 3),
        new Biome(0, 1, 0, 0), new Biome(2, 0, 3, 0), new Biome(2, 0, 2, 1), new Biome(2, 0, 1, 2),
        new Biome(0, 2, 0, 0), new Biome(1, 0, 2, 0), new Biome(1, 0, 2, 0), new Biome(1, 0, 1, 1),
        new Biome(0, 3, 0, 0), new Biome(1, 2, 1, 0), new Biome(1, 1, 1, 0), new Biome(1, 0, 0, 0)
    };
    [Range(0f, 1f)]
    float evaporation = 0.5f;
    [Range(0f, 1f)]
    float precipitationFactor = 0.25f;
    [Range(0f, 1f)]
    float evaporationFactor = 0.5f;
    [Range(0f, 1f)]
    float runoffFactor = 0.25f;
    [Range(0f, 1f)]
    float seepageFactor = 0.125f;
    HexDirection windDirection = HexDirection.NW;
    [Range(1f, 10f)]
    float windStrength = 4f;
    static float[] temperatureBands = { 0.1f, 0.3f, 0.6f };
    static float[] moistureBands = { 0.12f, 0.28f, 0.85f };
    [Range(0f, 1f)]
    float startingMoisture = 0.1f;
    [Range(0f, 1f)]
    float lowTemperature = 0f;
    [Range(0f, 1f)]
    float highTemperature = 1f;
    enum HemisphereMode {
        Both, North, South
    }
    HemisphereMode hemisphere = HemisphereMode.Both;
    [Range(0f, 1f)]
    float temperatureJitter = 0.1f;
    int temperatureJitterChannel;

    // ### Hexaoid update vars
    bool sink = false;
    int raiseLandIteration = 0;
    int sinkLandIteration = 0;
    float timeSinceLastLandCreation;
    float timeBetweenLandCreations = 0.3f;
    float timeSinceLastClimate;
    float timeBetweenClimates = 10f;
    public bool HexaoidClimateEvolution { get; set; }
    bool hexaoidCreation = true;

    public void ShowMap(bool show)  {
        grid.gameObject.SetActive(show);
    }

    public void GenerateMap (int x, int z) {
        Random.State originalRandomState = Random.state;

        if (!useFixedSeed) {
            seed = Random.Range(0, int.MaxValue);
            seed ^= (int)System.DateTime.Now.Ticks;
            seed ^= (int)Time.unscaledTime;
            seed &= int.MaxValue;
        }

        Random.InitState(seed);
        cellCount = x * z;
        grid.CreateMap(x, z);

        if (searchFrontier == null) {
            searchFrontier = new HexCellPriorityQueue();
        }

        for (int i = 0; i < cellCount; i++) {
            grid.GetCell(i).WaterLevel = waterLevel;
        }
        for (int i = 0; i < cellCount; i++) {
            grid.GetCell(i).SearchPhase = 0;
        }
        CreateRegion();
        Random.state = originalRandomState;
        underWaterCells = cellCount;
        minLandCells = Mathf.RoundToInt(cellCount * landPercentage * 0.01f);
        maxLandCells = Mathf.RoundToInt(minLandCells + ( minLandCells / 2f) );
    }

    void CreateRegion () {
            mapBorderX = Mathf.RoundToInt(grid.cellCountX * mapBorderPercentage * 0.01f);
            mapBorderZ = Mathf.RoundToInt(grid.cellCountZ * mapBorderPercentage * 0.01f);
            region.xMin = mapBorderX;
            region.xMax = grid.cellCountX - mapBorderX;
            region.zMin = mapBorderZ;
            region.zMax = grid.cellCountZ - mapBorderZ;
            int regionSize = (region.xMax - region.xMin) * (region.zMax - region.zMin);
            chunkSizeMin = Mathf.RoundToInt(regionSize / chunkSizeMinParts);
            chunkSizeMin = (chunkSizeMin < 1) ? 1 : chunkSizeMin;
            chunkSizeMax= Mathf.RoundToInt(regionSize / chunkSizeMaxParts);
            chunkSizeMax = (chunkSizeMax < 1) ? 1 : chunkSizeMax;
    }

    int GetUnderWaterCells() {
        int underWaterCells = 0;
        for (int i = 0; i < cellCount; i++) {
                HexCell cell = grid.GetCell(i);
                if (cell.IsUnderwater) {
                    underWaterCells++;
                }
        }
        return underWaterCells;
    }

    public void EvolveLandCreation (bool sink) {
        int getUnderWaterCells = GetUnderWaterCells();
        int chunkSize = Random.Range(chunkSizeMin, chunkSizeMax - 1);
        for (int i = 0; i < cellCount; i++) {
            grid.GetCell(i).SearchPhase = 0;
        }
        if (sink && (getUnderWaterCells <= cellCount)) {
            EvolveTerrainElevation(chunkSize, region, true);
        }
        else if (!sink && (cellCount - getUnderWaterCells) < maxLandCells) {
            EvolveTerrainElevation(chunkSize, region, false);
        }
    }

    public void SinkLand () {
        for (int i = 0; i < cellCount; i++) {
            HexCell current = grid.GetCell(i);
            current.Elevation = elevationMinimum; 
        }
    }
 
    public void CreateHole () {
        Stack<HexCell> nextFrontier = new Stack<HexCell>();
        Stack<HexCell> holeFrontier = new Stack<HexCell>();
        for (int i = 0; i < cellCount; i++) {
            HexCell initCurrent = grid.GetCell(i);
            initCurrent.Elevation = 0;
            initCurrent.SearchPhase = 0;
        }
        int holeCellSize = Mathf.RoundToInt(grid.cellCountX * grid.cellCountZ * 0.5f);
        HexCell centerCell;
        if(grid.cellCountZ % 2 == 0) {
            centerCell = grid.GetCell(Mathf.RoundToInt((grid.cellCountX * grid.cellCountZ * 0.5f) + (grid.cellCountX* 0.5f)));
        } else {
            centerCell = grid.GetCell(Mathf.RoundToInt((grid.cellCountX * grid.cellCountZ * 0.5f)));
        }
        
        holeFrontier.Push(centerCell);
        while (holeFrontier.Count < holeCellSize && holeFrontier.Count < maxHoleCellSize && holeFrontier.Count > 0) {
            while (holeFrontier.Count > 0) {
                HexCell current = holeFrontier.Pop();
                if(current) {
                    bool setNewElevation;
                    int newElevation = 0;
                    newElevation = current.Elevation - 3;
                    setNewElevation =  (newElevation >= elevationHole);
                    if (setNewElevation) {
                        current.Elevation = newElevation;
                        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
                            HexCell neighbor = current.GetNeighbor(d);
                            if(neighbor && neighbor.SearchPhase == 0) {
                                neighbor.SearchPhase = 1;
                                nextFrontier.Push(neighbor);
                            }
                        }
                    }
                } else {
                    Debug.Log("current not found");
                }
            }
            while (nextFrontier.Count > 0) {
                HexCell current = nextFrontier.Pop();
                current.SearchPhase = 0;
                holeFrontier.Push(current);
            }
        }
        nextFrontier.Clear();
        searchFrontier.Clear();
    }

    void EvolveTerrainElevation (int chunkSize, MapRegion region, bool sinkTerrain) {
        searchFrontierPhase += 1;
        HexCell firstCell = GetRandomCell(region);
        firstCell.SearchPhase = searchFrontierPhase;
        firstCell.Distance = 0;
        firstCell.SearchHeuristic = 0;
        searchFrontier.Enqueue(firstCell);
        HexCoordinates center = firstCell.coordinates;
        int rise = Random.value < highRiseProbability ? highRiseElevation : 1;
        int sink = Random.value < highRiseProbability ? highSinkElevation : 1;

        int size = 0;
        while (size < chunkSize && searchFrontier.Count > 0) {
            HexCell current = searchFrontier.Dequeue();
            bool setNewElevation;
            int newElevation = 0;

            if (sinkTerrain) {    
                int originalElevation = current.Elevation;
                newElevation = current.Elevation - sink;
                setNewElevation =  (newElevation >= elevationMinimum);
            } else {
                int originalElevation = current.Elevation;
                newElevation = current.Elevation + rise;
                setNewElevation = (newElevation <= elevationMaximum);
            }

            if (setNewElevation) {
                current.Elevation = newElevation;
                size += 1;
                addFrontierNeighbors(current, center);
            }
        }
        searchFrontier.Clear();
    }

    void addFrontierNeighbors(HexCell current, HexCoordinates center) {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
            HexCell neighbor = current.GetNeighbor(d);
            if (neighbor && neighbor.SearchPhase < searchFrontierPhase) {
                neighbor.SearchPhase = searchFrontierPhase;
                neighbor.Distance = neighbor.coordinates.DistanceTo(center);
                neighbor.SearchHeuristic = Random.value < jitterProbability ? 1: 0;
                searchFrontier.Enqueue(neighbor);
            }
        }
    }

    public void SetTerrainType () {
        temperatureJitterChannel = Random.Range(0, 4);
        int warmElevation =
            elevationMaximum - (elevationMaximum - waterLevel) / 2;

        for (int i = 0; i < cellCount; i++) {
            HexCell cell = grid.GetCell(i);
            float temperature = DetermineTemperature(cell);
            float moisture = climate[i].moisture;

            if (!cell.IsUnderwater) {
                int biomeTemperatureIndex = 0;
                for (; biomeTemperatureIndex < temperatureBands.Length; biomeTemperatureIndex++) {
                    if (temperature < temperatureBands[biomeTemperatureIndex]) {
                        break;
                    }
                }
                int biomeMoistureIndex = 0;
                for (; biomeMoistureIndex < moistureBands.Length; biomeMoistureIndex++) {
                    if (moisture < moistureBands[biomeMoistureIndex]) {
                        break;
                    }
                }
                Biome cellBiome = biomes[biomeTemperatureIndex * 4 + biomeMoistureIndex];

                if (cellBiome.terrain == 0) {
                    if (cell.Elevation >= warmElevation) {
                        cellBiome.terrain = 3;
                    }
                }
                else if (cell.Elevation == elevationMaximum) {
                    cellBiome.terrain = 4;
                }

                cell.TerrainTypeIndex = cellBiome.terrain;
                cell.FirstDetailLevel = cellBiome.warmPlant;
                cell.SecondDetailLevel = cellBiome.temperatePlant;
                cell.ThirdDetailLevel = cellBiome.polarPlant;
            }
            else {
                int terrain;
                if (cell.Elevation == waterLevel - 1) {
                    terrain = 1;
                }
                else if (cell.Elevation >= waterLevel) {
                    terrain = 1;
                }
                else if (cell.Elevation < 0) {
                    terrain = 3;
                }
                else {
                    terrain = 2;
                }
                if (terrain == 1 && temperature < temperatureBands[0]) {
                    terrain = 2;
                }
                cell.TerrainTypeIndex = terrain;
            }
        }
    }

    public void CreateClimate () {
        HexaoidClimateEvolution = true;
        climate.Clear();
        nextClimate.Clear();
        ClimateData initialData = new ClimateData();
        initialData.moisture = startingMoisture;
        ClimateData clearData = new ClimateData();
        for (int i = 0; i < cellCount; i++) {
            climate.Add(initialData);
            nextClimate.Add(clearData);
        }
        for (int cycle = 0; cycle < 40; cycle++) {
            for (int i = 0; i < cellCount; i++) {
                EvolveClimateCell(i);
            }
            List<ClimateData> swap = climate;
            climate = nextClimate;
            nextClimate = swap;
        }
    }

    public void EvolveClimate() {
        for (int i = 0; i < cellCount; i++) {
            EvolveClimateCell(i);
        }
        List<ClimateData> swap = climate;
        climate = nextClimate;
        nextClimate = swap;
    }

    void EvolveClimateCell (int cellIndex) {
        HexCell cell = grid.GetCell(cellIndex);
        ClimateData cellClimate = climate[cellIndex];
        
        if (cell.IsUnderwater) {
            cellClimate.moisture = 1f;
            cellClimate.clouds += evaporation;
        }
        else {
            float evaporation = cellClimate.moisture * evaporationFactor;
            cellClimate.moisture -= evaporation;
            cellClimate.clouds += evaporation;
        }
        
        float precipitation = cellClimate.clouds * precipitationFactor;
        cellClimate.clouds -= precipitation;
        cellClimate.moisture += precipitation;

        float cloudMaximum = 1f - cell.ViewElevation / (elevationMaximum + 1f);
        if (cellClimate.clouds > cloudMaximum) {
            cellClimate.moisture += cellClimate.clouds - cloudMaximum;
            cellClimate.clouds = cloudMaximum;
        }

        HexDirection mainDispersalDirection = windDirection.Opposite();
        float cloudDispersal = cellClimate.clouds * (1f / (5f + windStrength));
        float runoff = cellClimate.moisture * runoffFactor * (1f / 6f);
        float seepage = cellClimate.moisture * seepageFactor * (1f / 6f);
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
            HexCell neighbor = cell.GetNeighbor(d);
            if (!neighbor) {
                continue;
            }
            ClimateData neighborClimate = nextClimate[neighbor.Index];
            if (d == mainDispersalDirection) {
                neighborClimate.clouds += cloudDispersal * windStrength;
            }
            else {
                neighborClimate.clouds += cloudDispersal;
            }

            int elevationDelta = neighbor.ViewElevation - cell.ViewElevation;
            if (elevationDelta < 0) {
                cellClimate.moisture -= runoff;
                neighborClimate.moisture += runoff;
            }
            else if (elevationDelta == 0) {
                cellClimate.moisture -= seepage;
                neighborClimate.moisture += seepage;
            }

            nextClimate[neighbor.Index] = neighborClimate;
        }
        ClimateData nextCellClimate = nextClimate[cellIndex];
        nextCellClimate.moisture += cellClimate.moisture;
        if (nextCellClimate.moisture > 1f) {
            nextCellClimate.moisture = 1f;
        }
        nextClimate[cellIndex] = nextCellClimate;
        climate[cellIndex] = new ClimateData();

    }

    float DetermineTemperature (HexCell cell) {
        float latitude = (float)cell.coordinates.Z / grid.cellCountZ;
        if (hemisphere == HemisphereMode.Both) {
            latitude *= 2f;
            if (latitude > 1f) {
                latitude = 2f - latitude;
            }
        }
        else if (hemisphere == HemisphereMode.North) {
            latitude = 1f - latitude;
        }
        float temperature = Mathf.LerpUnclamped(lowTemperature, highTemperature, latitude);
        temperature *= 1f - (cell.ViewElevation - waterLevel) / (elevationMaximum - waterLevel + 1f);
        float jitter = HexMetrics.SampleNoise(cell.Position * 0.1f)[temperatureJitterChannel];
        temperature += (jitter * 2f - 1f) * temperatureJitter;
        return temperature;
    }

    HexCell GetRandomCell (MapRegion region) {
        return grid.GetCell(
            Random.Range(region.xMin, region.xMax),
            Random.Range(region.zMin, region.zMax)
        );
    }

    public void GenerateHexaoid(int raiseLandIterations, int sinkLandIterations) {
        raiseLandIteration = raiseLandIterations;
        sinkLandIteration = sinkLandIterations;
        // HexaoidClimateEvolution = true;
        timeSinceLastLandCreation = 0;
        timeSinceLastClimate = 0;
        hexaoidCreation = true;
    }

    public void SinkHexaoid() {
        hexaoidCreation = false;
        SinkLand();
    }

    void FixedUpdate()
    {
        if(HexaoidClimateEvolution) {
            timeSinceLastClimate += Time.deltaTime;
            if (timeSinceLastClimate >= timeBetweenClimates) {
                timeSinceLastClimate -= timeBetweenClimates;
                    EvolveClimate();
                    SetTerrainType();
            }
        }

        if (hexaoidCreation) {
            timeSinceLastLandCreation += Time.deltaTime;
            if (timeSinceLastLandCreation >= timeBetweenLandCreations) {
                timeSinceLastLandCreation -= timeBetweenLandCreations;
                if (raiseLandIteration > 0) {
                    sink = false;
                    raiseLandIteration--;
                    EvolveLandCreation(sink);
                } else {
                    if (sinkLandIteration > 0) {
                        sink = true;
                        sinkLandIteration--;
                        EvolveLandCreation(sink);
                    } else {
                        hexaoidCreation = false;
                    }
                }
            }
        }
    }
}
