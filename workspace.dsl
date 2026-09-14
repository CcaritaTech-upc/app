workspace "IoBuild" "Diagramas C4" {

    model {
        builder = person "Constructor" "Gestiona proyectos"
        owner = person "Propietario" "Controla su unidad"

        iobuild = softwareSystem "IoBuild" "Plataforma IoT" {
            landingPage = container "Landing" "Web" "HTML"
            webApp = container "Web SPA" "Frontend" "Vue 3"
            mobileApp = container "Mobile App" "Movil" "Kotlin"
            nginx = container "Nginx" "Proxy" "Nginx"

            api = container "API Monolito" "Backend" ".NET 9" {
                iamModule = component "IAM" "Auth" "ASP.NET"
                projModule = component "Publishing" "Obras" "ASP.NET"
                devModule = component "Devices" "IoT" "ASP.NET"
                subModule = component "Subscriptions" "Cobros" "ASP.NET"
                analyticsModule = component "Analytics" "Consumo" "ASP.NET"
                profModule = component "Profiles" "Perfiles" "ASP.NET"
                persistenceModule = component "Persistence" "ORM" "EF Core"
            }

            mosquitto = container "Mosquitto" "Broker" "Mosquitto"
            database = container "MySQL DB" "Datos" "MySQL 8" "Database"
        }

        iotDevices = softwareSystem "Dispositivos IoT" "Sensores"
        stripe = softwareSystem "Stripe" "Pagos"
        cloudinary = softwareSystem "Cloudinary" "Fotos"
        influxdb = softwareSystem "InfluxDB" "Telemetria"
        jaeger = softwareSystem "Jaeger" "Trazas"

        builder -> landingPage "Visita" "HTTPS"
        landingPage -> webApp "Redirige a" "HTTPS"
        builder -> webApp "Ingresa a" "HTTPS"
        builder -> mobileApp "Usa" "Android UI"

        owner -> webApp "Usa" "HTTPS"
        owner -> mobileApp "Usa" "Android UI"

        webApp -> nginx "Peticiones" "HTTPS"
        mobileApp -> nginx "Peticiones" "HTTPS"
        nginx -> webApp "Estaticos" "HTTP"

        nginx -> iamModule "Enruta /auth a" "HTTP"
        nginx -> projModule "Enruta /projects a" "HTTP"
        nginx -> devModule "Enruta /devices a" "HTTP"
        nginx -> subModule "Enruta /plans a" "HTTP"
        nginx -> analyticsModule "Enruta /analytics a" "HTTP"
        nginx -> profModule "Enruta /profiles a" "HTTP"

        iamModule -> persistenceModule "Guarda usuarios"
        projModule -> persistenceModule "Guarda proyectos"
        devModule -> persistenceModule "Guarda dispositivos"
        subModule -> persistenceModule "Guarda suscripciones"
        analyticsModule -> persistenceModule "Consulta datos"
        profModule -> persistenceModule "Guarda perfiles"

        persistenceModule -> database "Queries" "MySQL :3306"

        devModule -> mosquitto "MQTT" "MQTT :1883"
        subModule -> stripe "Checkout" "HTTPS"
        stripe -> subModule "Webhooks" "HTTPS"
        profModule -> cloudinary "Fotos" "HTTPS"
        analyticsModule -> influxdb "Metricas" "HTTP :8086"
        api -> jaeger "Trazas" "OTLP :4317"

        webApp -> cloudinary "Fotos" "HTTPS"
        webApp -> stripe "Checkout" "HTTPS"

        iotDevices -> mosquitto "Telemetria" "MQTT :1883"
        mosquitto -> iotDevices "Comandos" "MQTT :1883"
    }

    views {
        systemContext iobuild "Contexto" "Nivel 1: Contexto" {
            include *
            autoLayout lr
        }

        container iobuild "Contenedores" "Nivel 2: Contenedores" {
            include *
            autoLayout lr
        }

        component api "Componentes" "Nivel 3: Componentes" {
            include *
            autoLayout tb
        }

        styles {
            element "Person" {
                shape Person
                background #08427B
                color #ffffff
            }
            element "Software System" {
                background #1168BD
                color #ffffff
            }
            element "Container" {
                background #2A72C9
                color #ffffff
            }
            element "Component" {
                background #438DD5
                color #ffffff
            }
            element "Database" {
                shape Cylinder
                background #1F618D
                color #ffffff
            }
        }
    }
}
