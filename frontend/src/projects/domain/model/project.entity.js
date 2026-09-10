import { PROJECT_STATUS_LABELS } from "./project-status.enum.js";

export class Project {
    constructor({
                    id = null,
                    name = "",
                    description = "",
                    location = "",
                    totalUnits = 0,
                    occupiedUnits = 0,
                    status = "active",
                    builderId = null,
                    createdDate = null,
                    imageUrl = "",
                    structureDefined = false,
                }) {
        this.id = id;
        this.name = name;
        this.description = description;
        this.location = location;
        this.totalUnits = Number(totalUnits) || 0;
        this.occupiedUnits = Number(occupiedUnits) || 0;
        this.status = status;
        this.builderId = builderId;
        this.createdDate = createdDate;
        this.imageUrl = imageUrl;
        this.structureDefined = Boolean(structureDefined);
    }

    // Display-only label for the numeric backend status enum. Keeps `status`
    // raw (numeric) so it round-trips correctly on update (PUT expects an int).
    get statusLabel() {
        if (typeof this.status === 'number') {
            return PROJECT_STATUS_LABELS[this.status] ?? 'Unknown';
        }
        if (this.structureDefined) return 'On going';
        return this.status || 'Planned';
    }

    get occupancyRate() {
        if (!this.totalUnits || this.totalUnits <= 0) return 0;
        return Math.min(100, Math.round((this.occupiedUnits / this.totalUnits) * 100));
    }
}
