import { Project } from "../domain/model/project.entity.js";

export class ProjectAssembler {
    static toEntityFromResource(resource) {
        if (!resource) return null;
        return new Project({
            id: resource.id ?? resource.Id,
            name: resource.name ?? resource.Name ?? '',
            description: resource.description ?? resource.Description ?? '',
            location: resource.location ?? resource.Location ?? '',
            totalUnits: resource.totalUnits ?? resource.TotalUnits ?? 0,
            occupiedUnits: resource.occupiedUnits ?? resource.OccupiedUnits ?? 0,
            status: resource.status ?? (resource.structureDefined || resource.StructureDefined ? 1 : 0),
            builderId: resource.builderId ?? resource.BuilderId,
            createdDate: resource.createdDate || resource.createdAt || resource.CreatedAt || null,
            imageUrl: resource.imageUrl ?? resource.ImageUrl ?? '',
            structureDefined: Boolean(resource.structureDefined ?? resource.StructureDefined),
        });
    }

    static toEntitiesFromResponse(response) {
        if (!response || (!response.status && !response.data)) {
            console.error(`${response.status} - ${response.statusText}`);
            return [];
        }

        const resources = response.data || [];
        return resources.map((r) => this.toEntityFromResource(r));
    }
}
