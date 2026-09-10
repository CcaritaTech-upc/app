/**
 * Plan Entity
 * Represents a subscription plan with its details and features
 */
export class Plan {
    constructor({
        id = 0,
        name = '',
        price = 0,
        description = '',
        features = [],
        featuresJson = '',
        maxDevices = 0,
        maxAdministrators = 0,
        supportLevel = '',
        hasAPI = false,
        hasAnalytics = false
    }) {
        this.id = id;
        this.name = name;
        this.price = price;
        this.description = description;
        this.featuresJson = featuresJson;

        let parsedFeatures = [];
        if (Array.isArray(features) && features.length > 0) {
            parsedFeatures = features;
        } else if (typeof featuresJson === 'string' && featuresJson.trim()) {
            try {
                parsedFeatures = JSON.parse(featuresJson);
            } catch (_) {
                parsedFeatures = [];
            }
        } else if (typeof features === 'string' && features.trim()) {
            try {
                parsedFeatures = JSON.parse(features);
            } catch (_) {
                parsedFeatures = [];
            }
        }
        this.features = Array.isArray(parsedFeatures) ? parsedFeatures : [];

        this.maxDevices = maxDevices;
        this.maxAdministrators = maxAdministrators;
        this.supportLevel = supportLevel;
        this.hasAPI = hasAPI;
        this.hasAnalytics = hasAnalytics;
    }

    /**
     * Get formatted price per month
     */
    getFormattedPrice() {
        return `$${this.price}`;
    }

    /**
     * Check if it's an enterprise plan
     */
    isEnterprise() {
        return this.name.toLowerCase() === 'enterprise';
    }

    /**
     * Check if it's a professional plan
     */
    isProfessional() {
        return this.name.toLowerCase() === 'professional';
    }
}
