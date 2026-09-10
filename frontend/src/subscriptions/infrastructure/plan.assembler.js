import { Plan } from "../domain/model/plan.entity.js";

/**
 * Plan Assembler
 * Handles the conversion between API resources and Plan entities
 */
export class PlanAssembler {
    /**
     * Convert API resource to Plan entity
     */
    static toEntityFromResource(resource) {
        if (!resource) return null;

        let features = [];
        if (Array.isArray(resource.features)) {
            features = resource.features;
        } else if (typeof resource.featuresJson === 'string' && resource.featuresJson.trim()) {
            try {
                features = JSON.parse(resource.featuresJson);
            } catch (_) {
                features = [];
            }
        } else if (typeof resource.features === 'string' && resource.features.trim()) {
            try {
                features = JSON.parse(resource.features);
            } catch (_) {
                features = [];
            }
        }

        return new Plan({
            id: resource.id,
            name: resource.name,
            price: resource.price,
            description: resource.description,
            features: features,
            featuresJson: resource.featuresJson || (resource.features ? JSON.stringify(resource.features) : ''),
            maxDevices: resource.maxDevices,
            maxAdministrators: resource.maxAdministrators,
            supportLevel: resource.supportLevel,
            hasAPI: resource.hasAPI,
            hasAnalytics: resource.hasAnalytics
        });
    }

    /**
     * Convert API response to Plan entity
     */
    static toEntityFromResponse(response) {
        if (!response.data) return null;
        return PlanAssembler.toEntityFromResource(response.data);
    }

    /**
     * Convert array of API resources to Plan entities
     */
    static toEntitiesFromResourceArray(resources) {
        if (!Array.isArray(resources)) return [];
        return resources.map(resource => PlanAssembler.toEntityFromResource(resource));
    }

    /**
     * Convert Plan entity to API resource
     */
    static toResourceFromEntity(entity) {
        return {
            id: entity.id,
            name: entity.name,
            price: entity.price,
            description: entity.description,
            features: entity.features,
            maxDevices: entity.maxDevices,
            maxAdministrators: entity.maxAdministrators,
            supportLevel: entity.supportLevel,
            hasAPI: entity.hasAPI,
            hasAnalytics: entity.hasAnalytics
        };
    }
}

