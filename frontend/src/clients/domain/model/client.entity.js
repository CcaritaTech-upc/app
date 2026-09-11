export class Client {
    constructor({
                    id = null,
                    fullName = "",
                    projectId = 0,
                    projectName = "",
                    accountStatement = "Active",
                    email = "",
                    phoneNumber = "",
                    address = "",
                    unitId = null,
                    unitNumber = "",
                    deviceCount = 0
                }) {
        this.id = id;
        this.fullName = fullName;
        this.projectId = projectId;
        this.projectName = projectName;
        this.accountStatement = accountStatement;
        this.email = email;
        this.phoneNumber = phoneNumber;
        this.address = address;
        this.unitId = unitId;
        this.unitNumber = unitNumber;
        this.deviceCount = deviceCount;
    }
}

