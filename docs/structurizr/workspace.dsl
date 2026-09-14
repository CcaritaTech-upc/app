workspace "IoBuild Platform" "Diagramas de Arquitectura C4 para IoBuild Platform" {

    model {
        builder = person "Constructor / Inmobiliaria" "Crea proyectos inmobiliarios, define pisos/unidades, gestiona suscripciones de planes y analiza metricas." "Person"
        owner = person "Propietario / Residente" "Monitorea su unidad habitacional, registra dispositivos IoT y envia ordenes de control remoto." "Person"

        iobuild = softwareSystem "IoBuild Platform" "Plataforma centralizada de gestion de edificaciones inteligentes, automatizacion domotica y control energetico." "MainSystem" {
            landingPage = container "Landing Page" "Sitio web publico que presenta la propuesta de valor y capta constructores." "HTML / CSS / JS" "Web"
            webApp = container "Web SPA" "Plataforma web donde los usuarios gestionan proyectos, suscripciones y dispositivos." "Vue 3, Vite, PrimeVue" "Web"
            mobileApp = container "Mobile App" "App movil nativa para que propietarios y constructores gestionen dispositivos." "Android, Kotlin" "Mobile"
            nginx = container "Nginx (Reverse Proxy)" "Expuesto en puerto 80: entrega estaticos de la SPA y redirige peticiones /api al backend." "Nginx / Alpine" "Proxy"

            api = container "API Monolito (IoBuild.Api)" "Backend monolitico modular que aloja IAM, Publishing, Devices, Subscriptions, Analytics y Profiles." "ASP.NET Core 9, C#" "Backend" {
                iamModule = component "Modulo IAM" "Autenticacion, hashing BCrypt, emision de tokens JWT, revoked tokens y registro de usuarios." "ASP.NET Core"
                projModule = component "Modulo Publishing" "Gestion de proyectos inmobiliarios, estructuras de pisos y departamentos, y asignacion de propietarios." "ASP.NET Core"
                devModule = component "Modulo Devices & IoT" "Registro de dispositivos, envio de comandos autorizados y puente de transporte MQTT bidireccional." "ASP.NET Core"
                subModule = component "Modulo Subscriptions" "Catalogo de planes, creacion de sesiones checkout con Stripe y procesamiento de webhooks de pago." "ASP.NET Core"
                analyticsModule = component "Modulo Analytics" "Consultas analiticas de consumo energetico en vivo, proyecciones LWW y sink de series de tiempo." "ASP.NET Core"
                profModule = component "Modulo Profiles" "Perfiles de usuarios (builders/owners) y orquestacion de subida de fotos a Cloudinary." "ASP.NET Core"
                persistenceModule = component "Modulo Persistence (IoBuildDbContext)" "Unit of Work centralizado que gestiona DbSets, mapeos de entidades, transacciones y migraciones consolidadas." "Entity Framework Core"
            }

            mosquitto = container "Mosquitto (MQTT Broker)" "Broker pub/sub para mensajes de telemetria (telemetry/#) y ordenes (commands/#)." "Eclipse Mosquitto" "Broker"
            database = container "MySQL Database" "Instancia centralizada de base de datos relacional para todas las tablas del sistema IoBuild." "MySQL 8.0" "Database"
        }

        stripe = softwareSystem "Stripe" "Pasarela de pagos para checkout y suscripciones recurrentes." "External System"
        cloudinary = softwareSystem "Cloudinary" "Servicio cloud para subida, optimizacion y entrega de fotos de proyectos y perfiles." "External System"
        iotDevices = softwareSystem "Dispositivos IoT y Sensores" "Sensores de energia/temperatura y actuadores en departamentos (HVAC, luces)." "External System"
        influxdb = softwareSystem "InfluxDB (Time-Series DB)" "Base de datos especializada para telemetria de dispositivos IoT de alta frecuencia." "External System"
        jaeger = softwareSystem "Jaeger / OpenTelemetry" "Plataforma de observabilidad y trazado distribuido del sistema." "External System"

        # Relaciones de Personas con Contenedores
        builder -> landingPage "Visita para conocer" "HTTPS"
        landingPage -> webApp "Redirige a login" "HTTPS"
        builder -> webApp "Ingreso directo" "HTTPS"
        builder -> mobileApp "Usa app movil" "Android UI"

        owner -> webApp "Usa plataforma" "HTTPS"
        owner -> mobileApp "Usa app movil" "Android UI"

        # Relaciones de Contenedores Frontend con Nginx Proxy
        webApp -> nginx "Peticiones /api" "HTTPS"
        mobileApp -> nginx "Peticiones /api" "HTTPS"
        nginx -> webApp "Sirve archivos SPA" "HTTP"
        nginx -> api "Proxy inverso a :8080" "HTTP"

        # Relaciones Nginx a Componentes de Modulo
        nginx -> iamModule "Enruta /auth" "HTTP"
        nginx -> projModule "Enruta /projects" "HTTP"
        nginx -> devModule "Enruta /devices" "HTTP"
        nginx -> subModule "Enruta /subscriptions" "HTTP"
        nginx -> analyticsModule "Enruta /analytics" "HTTP"
        nginx -> profModule "Enruta /profiles" "HTTP"

        # Relaciones de Componentes con Persistencia
        iamModule -> persistenceModule "Guarda usuarios"
        projModule -> persistenceModule "Guarda proyectos y unidades"
        devModule -> persistenceModule "Guarda dispositivos"
        subModule -> persistenceModule "Guarda planes y suscripciones"
        analyticsModule -> persistenceModule "Consulta proyecciones"
        profModule -> persistenceModule "Guarda perfiles"

        persistenceModule -> database "Ejecuta queries y transacciones SQL" "MySQL Protocol :3306"

        # Relaciones con Sistemas Externos y Broker
        webApp -> stripe "Redirige a checkout" "HTTPS / Stripe.js"
        subModule -> stripe "Crea sesiones checkout" "HTTPS / REST"
        stripe -> subModule "Webhooks de cobro" "HTTPS"

        webApp -> cloudinary "Sube/descarga fotos" "HTTPS"
        profModule -> cloudinary "Sube fotos de perfil" "HTTPS / REST"

        devModule -> mosquitto "Publica comandos y suscribe" "MQTT :1883"
        mosquitto -> devModule "Entrega telemetria recibida" "MQTT :1883"

        analyticsModule -> influxdb "Ingesta telemetria" "HTTP :8086"
        api -> jaeger "Envia trazas OTLP" "gRPC :4317"

        iotDevices -> mosquitto "Publica lecturas sensor" "MQTT :1883"
        mosquitto -> iotDevices "Recibe comandos actuador" "MQTT :1883"
    }

    views {
        systemContext iobuild "Contexto" "Nivel 1: Contexto del Sistema" {
            include *
            autoLayout lr
        }

        container iobuild "Contenedores" "Nivel 2: Contenedores del Sistema" {
            include *
            autoLayout lr
        }

        component api "Componentes" "Nivel 3: Componentes del API Monolito" {
            include *
            include nginx
            include database
            include mosquitto
            include stripe
            include cloudinary
            include influxdb
            autoLayout tb
        }

        styles {
            element "Person" {
                shape Person
                background #08427B
                color #ffffff
            }
            element "MainSystem" {
                background #1168BD
                color #ffffff
            }
            element "External System" {
                background #8A9BA8
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
