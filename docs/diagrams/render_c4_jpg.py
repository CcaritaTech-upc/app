import os
import subprocess
from PIL import Image

EDGE_PATH = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
OUTPUT_DIR = r"C:\Users\axelm\OneDrive\Documentos\MisArchivos\Upc-202602\Diseño\docs\diagrams"

def person_box(x, y, w, h, name, desc):
    return f"""
    <g transform="translate({x},{y})">
        <circle cx="{w/2}" cy="-16" r="22" fill="#08427B" stroke="#052e56" stroke-width="2"/>
        <rect x="0" y="0" width="{w}" height="{h}" rx="14" fill="#08427B" stroke="#052e56" stroke-width="2.5" filter="url(#shadow)"/>
        <text x="{w/2}" y="28" font-family="'Segoe UI', Roboto, sans-serif" font-size="18" font-weight="700" fill="#ffffff" text-anchor="middle">{name}</text>
        <text x="{w/2}" y="48" font-family="'Segoe UI', Roboto, sans-serif" font-size="12" fill="#b0cde8" text-anchor="middle">[Persona / Usuario]</text>
        <foreignObject x="10" y="58" width="{w-20}" height="{h-65}">
            <div xmlns="http://www.w3.org/1999/xhtml" style="font-family:'Segoe UI', Roboto, sans-serif; font-size:12px; color:#e0effc; text-align:center; line-height:1.35;">
                {desc}
            </div>
        </foreignObject>
    </g>
    """

def system_box(x, y, w, h, name, desc, is_external=False):
    bg = "#8A9BA8" if is_external else "#1168BD"
    border = "#5f717d" if is_external else "#0c4f91"
    sub_color = "#e6edf2" if is_external else "#b7d6f7"
    type_label = "[Sistema Externo]" if is_external else "[Sistema Principal]"
    return f"""
    <g transform="translate({x},{y})">
        <rect x="0" y="0" width="{w}" height="{h}" rx="14" fill="{bg}" stroke="{border}" stroke-width="2.5" filter="url(#shadow)"/>
        <text x="{w/2}" y="32" font-family="'Segoe UI', Roboto, sans-serif" font-size="19" font-weight="700" fill="#ffffff" text-anchor="middle">{name}</text>
        <text x="{w/2}" y="52" font-family="'Segoe UI', Roboto, sans-serif" font-size="12" fill="{sub_color}" text-anchor="middle">{type_label}</text>
        <foreignObject x="12" y="62" width="{w-24}" height="{h-70}">
            <div xmlns="http://www.w3.org/1999/xhtml" style="font-family:'Segoe UI', Roboto, sans-serif; font-size:13px; color:#ffffff; text-align:center; line-height:1.4;">
                {desc}
            </div>
        </foreignObject>
    </g>
    """

def container_box(x, y, w, h, name, tech, desc):
    return f"""
    <g transform="translate({x},{y})">
        <rect x="0" y="0" width="{w}" height="{h}" rx="12" fill="#2A72C9" stroke="#1c5599" stroke-width="2" filter="url(#shadow)"/>
        <text x="{w/2}" y="28" font-family="'Segoe UI', Roboto, sans-serif" font-size="17" font-weight="700" fill="#ffffff" text-anchor="middle">{name}</text>
        <text x="{w/2}" y="47" font-family="'Segoe UI', Roboto, sans-serif" font-size="11" fill="#cbe2fc" text-anchor="middle">[Contenedor: {tech}]</text>
        <foreignObject x="10" y="55" width="{w-20}" height="{h-60}">
            <div xmlns="http://www.w3.org/1999/xhtml" style="font-family:'Segoe UI', Roboto, sans-serif; font-size:12px; color:#ffffff; text-align:center; line-height:1.35;">
                {desc}
            </div>
        </foreignObject>
    </g>
    """

def database_box(x, y, w, h, name, tech, desc):
    return f"""
    <g transform="translate({x},{y})">
        <path d="M 0,20 L 0,{h-20} A {w/2},20 0 0 0 {w},{h-20} L {w},20 Z" fill="#1F618D" stroke="#154360" stroke-width="2.5" filter="url(#shadow)"/>
        <ellipse cx="{w/2}" cy="{h-20}" rx="{w/2}" ry="20" fill="#1F618D" stroke="#154360" stroke-width="2.5"/>
        <ellipse cx="{w/2}" cy="20" rx="{w/2}" ry="20" fill="#2980B9" stroke="#154360" stroke-width="2.5"/>
        <text x="{w/2}" y="60" font-family="'Segoe UI', Roboto, sans-serif" font-size="17" font-weight="700" fill="#ffffff" text-anchor="middle">{name}</text>
        <text x="{w/2}" y="80" font-family="'Segoe UI', Roboto, sans-serif" font-size="11" fill="#d4e6f1" text-anchor="middle">[Base de Datos: {tech}]</text>
        <foreignObject x="12" y="90" width="{w-24}" height="{h-100}">
            <div xmlns="http://www.w3.org/1999/xhtml" style="font-family:'Segoe UI', Roboto, sans-serif; font-size:12px; color:#ffffff; text-align:center; line-height:1.35;">
                {desc}
            </div>
        </foreignObject>
    </g>
    """

def component_box(x, y, w, h, name, tech, desc):
    return f"""
    <g transform="translate({x},{y})">
        <rect x="0" y="0" width="{w}" height="{h}" rx="10" fill="#438DD5" stroke="#2b6fb5" stroke-width="2" filter="url(#shadow)"/>
        <text x="{w/2}" y="26" font-family="'Segoe UI', Roboto, sans-serif" font-size="16" font-weight="700" fill="#ffffff" text-anchor="middle">{name}</text>
        <text x="{w/2}" y="44" font-family="'Segoe UI', Roboto, sans-serif" font-size="11" fill="#d9ecff" text-anchor="middle">[Componente: {tech}]</text>
        <foreignObject x="8" y="52" width="{w-16}" height="{h-56}">
            <div xmlns="http://www.w3.org/1999/xhtml" style="font-family:'Segoe UI', Roboto, sans-serif; font-size:11px; color:#ffffff; text-align:center; line-height:1.3;">
                {desc}
            </div>
        </foreignObject>
    </g>
    """

def draw_arrow(x1, y1, x2, y2, label="", protocol="", dashed=False, label_dx=0, label_dy=0, label_w=170):
    dash_attr = 'stroke-dasharray="6,5"' if dashed else ''
    mx = (x1 + x2) / 2 + label_dx
    my = (y1 + y2) / 2 + label_dy
    
    label_markup = ""
    if label or protocol:
        lines = []
        if label: lines.append(f'<div style="font-weight:600; color:#222; font-size:11px;">{label}</div>')
        if protocol: lines.append(f'<div style="font-weight:400; color:#555; font-size:10px; margin-top:2px;">[{protocol}]</div>')
        content = "".join(lines)
        label_markup = f"""
        <g transform="translate({mx - label_w/2}, {my - 20})">
            <rect width="{label_w}" height="38" rx="6" fill="#ffffff" stroke="#bbbbbb" stroke-width="1" filter="url(#mini-shadow)" opacity="0.96"/>
            <foreignObject x="0" y="2" width="{label_w}" height="34">
                <div xmlns="http://www.w3.org/1999/xhtml" style="font-family:'Segoe UI', sans-serif; text-align:center;">
                    {content}
                </div>
            </foreignObject>
        </g>
        """

    return f"""
    <path d="M {x1},{y1} L {x2},{y2}" stroke="#666666" stroke-width="2" {dash_attr} marker-end="url(#arrowhead)"/>
    {label_markup}
    """

def draw_curved_arrow(x1, y1, cx, cy, x2, y2, label="", protocol="", label_w=170):
    mx = (x1 + 2*cx + x2) / 4
    my = (y1 + 2*cy + y2) / 4
    label_markup = ""
    if label or protocol:
        lines = []
        if label: lines.append(f'<div style="font-weight:600; color:#222; font-size:11px;">{label}</div>')
        if protocol: lines.append(f'<div style="font-weight:400; color:#555; font-size:10px; margin-top:2px;">[{protocol}]</div>')
        content = "".join(lines)
        label_markup = f"""
        <g transform="translate({mx - label_w/2}, {my - 20})">
            <rect width="{label_w}" height="36" rx="6" fill="#ffffff" stroke="#bbbbbb" stroke-width="1" filter="url(#mini-shadow)" opacity="0.96"/>
            <foreignObject x="0" y="2" width="{label_w}" height="32">
                <div xmlns="http://www.w3.org/1999/xhtml" style="font-family:'Segoe UI', sans-serif; text-align:center;">
                    {content}
                </div>
            </foreignObject>
        </g>
        """
    return f"""
    <path d="M {x1},{y1} Q {cx},{cy} {x2},{y2}" stroke="#666666" stroke-width="2" fill="none" marker-end="url(#arrowhead)"/>
    {label_markup}
    """

# -------------------------------------------------------------
# 1. DIAGRAMA DE CONTEXTO (NIVEL 1)
# -------------------------------------------------------------
def build_context_svg():
    width, height = 1800, 1100
    svg = f"""
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {width} {height}" width="{width}" height="{height}" style="background-color:#f9fbfd;">
    <defs>
        <filter id="shadow" x="-5%" y="-5%" width="112%" height="116%" filterUnits="userSpaceOnUse">
            <feDropShadow dx="2" dy="5" stdDeviation="5" flood-color="#000000" flood-opacity="0.18"/>
        </filter>
        <filter id="mini-shadow" x="-5%" y="-5%" width="110%" height="120%" filterUnits="userSpaceOnUse">
            <feDropShadow dx="1" dy="2" stdDeviation="2" flood-color="#000000" flood-opacity="0.15"/>
        </filter>
        <marker id="arrowhead" markerWidth="10" markerHeight="7" refX="9" refY="3.5" orient="auto">
            <polygon points="0 0, 10 3.5, 0 7" fill="#666666"/>
        </marker>
    </defs>

    <rect x="40" y="30" width="{width-80}" height="70" rx="8" fill="#ffffff" stroke="#e1e8ed" stroke-width="1.5"/>
    <text x="70" y="66" font-family="'Segoe UI', Roboto, sans-serif" font-size="24" font-weight="700" fill="#1c3d5a">[Nivel 1: Contexto] IoBuild Platform - Sistema de Gestión Inmobiliaria e IoT</text>
    <text x="70" y="88" font-family="'Segoe UI', Roboto, sans-serif" font-size="14" fill="#6b7c96">Muestra los usuarios del sistema y cómo interactúa con servicios externos (Stripe, Cloudinary, InfluxDB, Jaeger, Dispositivos IoT)</text>

    {person_box(100, 240, 260, 150, "Constructor / Inmobiliaria", "Crea proyectos inmobiliarios, define pisos/unidades, gestiona suscripciones de planes y analiza métricas.")}
    {person_box(100, 680, 260, 150, "Propietario / Residente", "Monitorea su unidad habitacional, registra dispositivos IoT y envía órdenes de control remoto.")}

    {system_box(680, 430, 360, 230, "IoBuild Platform", "Plataforma centralizada de gestión de edificaciones inteligentes, automatización domótica y control energético.")}

    {system_box(1380, 150, 290, 150, "Stripe", "Pasarela de pagos para checkout y suscripciones recurrentes.", is_external=True)}
    {system_box(1380, 370, 290, 150, "Cloudinary", "Servicio cloud para subida, optimización y entrega de fotos de proyectos y perfiles.", is_external=True)}
    {system_box(1380, 590, 290, 150, "Dispositivos IoT y Sensores", "Sensores de energía/temperatura y actuadores en departamentos (HVAC, luces).", is_external=True)}
    {system_box(1380, 810, 290, 150, "InfluxDB (Time-Series DB)", "Base de datos especializada para telemetría de dispositivos IoT de alta frecuencia.", is_external=True)}
    {system_box(720, 850, 280, 140, "Jaeger / OpenTelemetry", "Plataforma de observabilidad y trazado distribuido del sistema.", is_external=True)}

    {draw_arrow(360, 315, 680, 480, "Gestiona proyectos y planes", "HTTPS", label_dx=-30, label_dy=-20)}
    {draw_arrow(360, 755, 680, 600, "Controla dispositivos y telemetría", "HTTPS", label_dx=-30, label_dy=20)}

    {draw_arrow(1040, 470, 1380, 240, "Crea checkout y cobra", "HTTPS / REST", label_dx=30, label_dy=-30)}
    {draw_arrow(1040, 510, 1380, 435, "Almacena y sirve fotos", "HTTPS", label_dx=20, label_dy=-10)}
    {draw_arrow(1040, 560, 1380, 630, "Envía comandos de control", "MQTT", label_dx=20, label_dy=-15)}
    {draw_arrow(1380, 680, 1040, 610, "Publica telemetría y estado", "MQTT", label_dx=20, label_dy=20)}
    {draw_arrow(1040, 640, 1380, 860, "Guarda telemetría histórica", "HTTP :8086", label_dx=30, label_dy=30)}
    {draw_arrow(860, 660, 860, 850, "Exporta trazas de ejecución", "OTLP / gRPC", label_dx=0, label_dy=0)}

    <g transform="translate(60, 990)">
        <rect width="650" height="60" rx="8" fill="#ffffff" stroke="#d0dbe5" stroke-width="1"/>
        <rect x="20" y="18" width="24" height="24" rx="4" fill="#08427B"/>
        <text x="52" y="35" font-family="'Segoe UI', sans-serif" font-size="13" font-weight="600" fill="#222222">Persona / Rol</text>
        <rect x="180" y="18" width="24" height="24" rx="4" fill="#1168BD"/>
        <text x="212" y="35" font-family="'Segoe UI', sans-serif" font-size="13" font-weight="600" fill="#222222">Sistema Principal (IoBuild)</text>
        <rect x="430" y="18" width="24" height="24" rx="4" fill="#8A9BA8"/>
        <text x="462" y="35" font-family="'Segoe UI', sans-serif" font-size="13" font-weight="600" fill="#222222">Sistema Externo</text>
    </g>
</svg>
"""
    return svg

# -------------------------------------------------------------
# 2. DIAGRAMA DE CONTENEDORES (NIVEL 2 - Espaciado Mejorado)
# -------------------------------------------------------------
def build_container_svg():
    width, height = 2400, 1400
    svg = f"""
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {width} {height}" width="{width}" height="{height}" style="background-color:#f8fafd;">
    <defs>
        <filter id="shadow" x="-5%" y="-5%" width="112%" height="116%" filterUnits="userSpaceOnUse">
            <feDropShadow dx="2" dy="5" stdDeviation="5" flood-color="#000000" flood-opacity="0.18"/>
        </filter>
        <filter id="mini-shadow" x="-5%" y="-5%" width="110%" height="120%" filterUnits="userSpaceOnUse">
            <feDropShadow dx="1" dy="2" stdDeviation="2" flood-color="#000000" flood-opacity="0.15"/>
        </filter>
        <marker id="arrowhead" markerWidth="10" markerHeight="7" refX="9" refY="3.5" orient="auto">
            <polygon points="0 0, 10 3.5, 0 7" fill="#666666"/>
        </marker>
    </defs>

    <rect x="50" y="30" width="{width-100}" height="70" rx="8" fill="#ffffff" stroke="#e1e8ed" stroke-width="1.5"/>
    <text x="80" y="66" font-family="'Segoe UI', Roboto, sans-serif" font-size="24" font-weight="700" fill="#1c3d5a">[Nivel 2: Contenedores] IoBuild Platform - Arquitectura de Contenedores</text>
    <text x="80" y="88" font-family="'Segoe UI', Roboto, sans-serif" font-size="14" fill="#6b7c96">Desglose de aplicaciones web, móvil, backend monolítico, proxy Nginx, bases de datos y broker MQTT</text>

    <!-- ACTORES (Izquierda) -->
    {person_box(80, 260, 240, 140, "Constructor", "Administra proyectos, unidades, compras y analíticas.")}
    {person_box(80, 680, 240, 140, "Propietario", "Monitorea y opera dispositivos IoT de su departamento.")}

    <!-- BOUNDARY: IoBuild Platform -->
    <g transform="translate(380, 150)">
        <rect x="0" y="0" width="1380" height="1160" rx="18" fill="#ffffff" stroke="#1168BD" stroke-width="2" stroke-dasharray="8,6"/>
        <text x="30" y="40" font-family="'Segoe UI', sans-serif" font-size="20" font-weight="700" fill="#1168BD">IoBuild Platform</text>
        <text x="30" y="62" font-family="'Segoe UI', sans-serif" font-size="13" fill="#6b7c96">[Límite del Sistema de Software]</text>
    </g>

    <!-- CONTENEDORES INTERNOS (Espaciados holgadamente) -->
    {container_box(430, 240, 230, 140, "Landing Page", "HTML / CSS / JS", "Sitio web público que presenta la propuesta de valor y capta constructores.")}
    {container_box(780, 240, 250, 140, "Web SPA", "Vue 3, Vite, PrimeVue", "Plataforma web donde los usuarios gestionan proyectos, suscripciones y dispositivos.")}
    {container_box(780, 480, 250, 140, "Mobile App", "Android, Kotlin", "App móvil nativa para que propietarios y constructores gestionen dispositivos.")}
    {container_box(1180, 330, 250, 150, "Nginx (Reverse Proxy)", "Nginx / Alpine", "Expuesto en puerto 80: entrega estáticos de la SPA y redirige peticiones /api al backend.")}

    {container_box(1150, 640, 310, 170, "API Monolito (IoBuild.Api)", "ASP.NET Core 9, C#", "Backend monolítico modular que aloja IAM, Publishing, Devices, Subscriptions, Analytics y Profiles.")}

    {database_box(920, 960, 270, 180, "MySQL DB", "MySQL 8.0", "Almacena usuarios, proyectos, unidades, dispositivos, perfiles y suscripciones.")}
    {container_box(1340, 970, 280, 160, "Mosquitto (MQTT Broker)", "Eclipse Mosquitto", "Broker pub/sub para mensajes de telemetría (telemetry/#) y órdenes (commands/#).")}

    <!-- SISTEMAS EXTERNOS (Derecha) -->
    {system_box(1900, 220, 260, 140, "Stripe", "Pasarela de pagos para checkout y webhooks de planes.", is_external=True)}
    {system_box(1900, 430, 260, 140, "Cloudinary", "Almacena y optimiza fotos de perfiles y proyectos.", is_external=True)}
    {system_box(1900, 650, 260, 140, "Jaeger / OTel", "Plataforma de observabilidad y trazas distribuidas.", is_external=True)}
    {system_box(1900, 870, 260, 140, "InfluxDB", "Base de datos time-series para telemetría de alta frecuencia.", is_external=True)}
    {system_box(1900, 1090, 260, 140, "Dispositivos IoT", "Sensores de energía y actuadores en departamentos.", is_external=True)}

    <!-- RELACIONES -->
    {draw_arrow(320, 310, 430, 310, "Visita para conocer", "HTTPS")}
    {draw_arrow(660, 310, 780, 310, "Redirige a login", "HTTPS", label_w=140)}
    {draw_curved_arrow(320, 330, 520, 400, 780, 340, "Ingreso directo", "HTTPS")}
    {draw_arrow(320, 370, 780, 520, "Usa app móvil", "Android UI")}

    {draw_arrow(320, 720, 780, 370, "Usa plataforma", "HTTPS")}
    {draw_arrow(320, 750, 780, 560, "Usa app móvil", "Android UI")}

    {draw_arrow(1030, 330, 1180, 370, "Peticiones /api", "HTTPS", label_w=140)}
    {draw_arrow(1030, 520, 1180, 430, "Peticiones /api", "HTTPS", label_w=140)}
    {draw_arrow(1180, 310, 1030, 270, "Sirve archivos SPA", "HTTP", label_w=140)}

    {draw_arrow(1300, 480, 1300, 640, "Proxy inverso a :8080", "HTTP", label_w=160)}

    {draw_arrow(1220, 810, 1070, 960, "Lectura/escritura datos", "MySQL :3306")}
    {draw_arrow(1380, 810, 1460, 970, "Publica/suscribe tópicos", "MQTT :1883")}

    {draw_curved_arrow(1030, 240, 1450, 140, 1900, 260, "Redirige a checkout", "HTTPS / Stripe.js")}
    {draw_arrow(1460, 690, 1900, 310, "Crea sesiones checkout", "HTTPS / REST")}
    {draw_curved_arrow(1900, 330, 1720, 550, 1460, 720, "Webhooks de cobro", "HTTPS")}

    {draw_curved_arrow(1030, 270, 1450, 290, 1900, 470, "Sube/descarga fotos", "HTTPS")}
    {draw_arrow(1460, 740, 1900, 520, "Gestiona fotos de perfil", "HTTPS / REST")}

    {draw_arrow(1460, 770, 1900, 920, "Ingesta telemetría", "HTTP :8086")}
    {draw_arrow(1460, 720, 1900, 720, "Envía trazas OTLP", "gRPC :4317")}

    {draw_arrow(1900, 1140, 1620, 1070, "Publica lecturas sensor", "MQTT :1883")}
    {draw_arrow(1620, 1100, 1900, 1170, "Recibe comandos actuador", "MQTT :1883")}

    <g transform="translate(60, 1300)">
        <rect width="900" height="60" rx="8" fill="#ffffff" stroke="#d0dbe5" stroke-width="1"/>
        <rect x="20" y="18" width="24" height="24" rx="4" fill="#08427B"/>
        <text x="52" y="35" font-family="'Segoe UI', sans-serif" font-size="13" font-weight="600" fill="#222222">Persona</text>
        <rect x="140" y="18" width="24" height="24" rx="4" fill="#2A72C9"/>
        <text x="172" y="35" font-family="'Segoe UI', sans-serif" font-size="13" font-weight="600" fill="#222222">Contenedor Web/App/API</text>
        <rect x="380" y="18" width="24" height="24" rx="4" fill="#1F618D"/>
        <text x="412" y="35" font-family="'Segoe UI', sans-serif" font-size="13" font-weight="600" fill="#222222">Base de Datos</text>
        <rect x="560" y="18" width="24" height="24" rx="4" fill="#8A9BA8"/>
        <text x="592" y="35" font-family="'Segoe UI', sans-serif" font-size="13" font-weight="600" fill="#222222">Sistema Externo</text>
    </g>
</svg>
"""
    return svg

# -------------------------------------------------------------
# 3. DIAGRAMA DE COMPONENTES (NIVEL 3 - Rutas Limpias)
# -------------------------------------------------------------
def build_component_svg():
    width, height = 2400, 1450
    svg = f"""
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {width} {height}" width="{width}" height="{height}" style="background-color:#f8fafd;">
    <defs>
        <filter id="shadow" x="-5%" y="-5%" width="112%" height="116%" filterUnits="userSpaceOnUse">
            <feDropShadow dx="2" dy="5" stdDeviation="5" flood-color="#000000" flood-opacity="0.18"/>
        </filter>
        <filter id="mini-shadow" x="-5%" y="-5%" width="110%" height="120%" filterUnits="userSpaceOnUse">
            <feDropShadow dx="1" dy="2" stdDeviation="2" flood-color="#000000" flood-opacity="0.15"/>
        </filter>
        <marker id="arrowhead" markerWidth="10" markerHeight="7" refX="9" refY="3.5" orient="auto">
            <polygon points="0 0, 10 3.5, 0 7" fill="#666666"/>
        </marker>
    </defs>

    <rect x="50" y="30" width="{width-100}" height="70" rx="8" fill="#ffffff" stroke="#e1e8ed" stroke-width="1.5"/>
    <text x="80" y="66" font-family="'Segoe UI', Roboto, sans-serif" font-size="24" font-weight="700" fill="#1c3d5a">[Nivel 3: Componentes] IoBuild Backend API - Arquitectura Monolítica Modular</text>
    <text x="80" y="88" font-family="'Segoe UI', Roboto, sans-serif" font-size="14" fill="#6b7c96">Desglose horizontal de módulos DDD dentro del contenedor IoBuild.Api en ASP.NET Core 9 y Entity Framework Core</text>

    <!-- ENTRADA DE TRÁFICO (Top): Nginx y Clientes -->
    {container_box(1070, 130, 260, 120, "Nginx (Reverse Proxy)", "Nginx / Alpine", "Enruta el tráfico entrante de Web SPA y Mobile App hacia los módulos correspondientes.")}

    <!-- BOUNDARY: API Monolito (IoBuild.Api) -->
    <g transform="translate(100, 310)">
        <rect x="0" y="0" width="2200" height="690" rx="18" fill="#ffffff" stroke="#2A72C9" stroke-width="2.5" stroke-dasharray="9,6"/>
        <text x="30" y="36" font-family="'Segoe UI', sans-serif" font-size="20" font-weight="700" fill="#2A72C9">API Monolito (IoBuild.Api)</text>
        <text x="30" y="58" font-family="'Segoe UI', sans-serif" font-size="13" fill="#6b7c96">[Límite del Contenedor Backend en ASP.NET Core 9]</text>
    </g>

    <!-- FILA HORIZONTAL DE LOS 6 MÓDULOS DE NEGOCIO (Y=420) -->
    {component_box(140, 420, 320, 180, "Módulo IAM", "ASP.NET Core", "Autenticación, hashing BCrypt, emisión de tokens JWT, revoked tokens y registro de usuarios.")}
    {component_box(500, 420, 320, 180, "Módulo Publishing", "ASP.NET Core", "Gestión de proyectos inmobiliarios, estructuras de pisos y departamentos, y asignación de propietarios.")}
    {component_box(860, 420, 320, 180, "Módulo Devices & IoT", "ASP.NET Core", "Registro de dispositivos, envío de comandos autorizados y puente de transporte MQTT bidireccional.")}
    {component_box(1220, 420, 320, 180, "Módulo Subscriptions", "ASP.NET Core", "Catálogo de planes, creación de sesiones checkout con Stripe y procesamiento de webhooks de pago.")}
    {component_box(1580, 420, 320, 180, "Módulo Analytics", "ASP.NET Core", "Consultas analíticas de consumo energético en vivo, proyecciones LWW y sink de series de tiempo.")}
    {component_box(1940, 420, 320, 180, "Módulo Profiles", "ASP.NET Core", "Perfiles de usuarios (builders/owners) y orquestación de subida de fotos a Cloudinary.")}

    <!-- COMPONENTE TRANSVERSAL DE PERSISTENCIA (Y=740) -->
    {component_box(820, 750, 760, 160, "Módulo Persistence (IoBuildDbContext)", "Entity Framework Core", "Unit of Work centralizado que gestiona DbSets, mapeos de entidades, transacciones y migraciones consolidadas.")}

    <!-- DEPENDENCIAS EXTERNAS E INFRAESTRUCTURA (Abajo) -->
    {system_box(140, 1120, 280, 140, "InfluxDB", "Almacena y consulta telemetría de energía de alta frecuencia.", is_external=True)}
    {container_box(540, 1120, 280, 140, "Mosquitto (Broker MQTT)", "Eclipse Mosquitto", "Intercambia mensajes MQTT de telemetría y comandos con los dispositivos.")}
    {database_box(990, 1100, 380, 190, "MySQL Database", "MySQL 8.0", "Instancia centralizada de base de datos relacional para todas las tablas del sistema IoBuild.")}
    {system_box(1490, 1120, 280, 140, "Stripe API", "Crea sesiones checkout y emite webhooks de confirmación.", is_external=True)}
    {system_box(1890, 1120, 280, 140, "Cloudinary API", "Almacena y optimiza las imágenes de perfil y proyectos.", is_external=True)}

    <!-- RELACIONES DESDE NGINX HACIA CADA MÓDULO -->
    {draw_arrow(1100, 250, 300, 420, "Enruta /auth", "HTTP", label_w=130)}
    {draw_arrow(1140, 250, 660, 420, "Enruta /projects", "HTTP", label_w=130)}
    {draw_arrow(1180, 250, 1020, 420, "Enruta /devices", "HTTP", label_w=130)}
    {draw_arrow(1220, 250, 1380, 420, "Enruta /subscriptions", "HTTP", label_w=140)}
    {draw_arrow(1260, 250, 1740, 420, "Enruta /analytics", "HTTP", label_w=130)}
    {draw_arrow(1300, 250, 2100, 420, "Enruta /profiles", "HTTP", label_w=130)}

    <!-- RELACIONES DESDE CADA MÓDULO HACIA PERSISTENCIA -->
    {draw_arrow(300, 600, 860, 750, "Guarda usuarios", label_w=130)}
    {draw_arrow(660, 600, 1000, 750, "Guarda proyectos y unidades", label_w=170)}
    {draw_arrow(1020, 600, 1140, 750, "Guarda dispositivos", label_w=140)}
    {draw_arrow(1380, 600, 1260, 750, "Guarda planes y suscripciones", label_w=180)}
    {draw_arrow(1740, 600, 1400, 750, "Consulta proyecciones", label_w=150)}
    {draw_arrow(2100, 600, 1540, 750, "Guarda perfiles", label_w=130)}

    <!-- PERSISTENCIA A MYSQL -->
    {draw_arrow(1200, 910, 1200, 1100, "Ejecuta queries y transacciones SQL", "MySQL Protocol :3306", label_w=230)}

    <!-- CONEXIONES EXTERNAS LIMPIAS (Laterales/Descendentes) -->
    <!-- Devices -> Mosquitto (Directo vertical) -->
    {draw_arrow(940, 600, 680, 1120, "Publica comandos y suscribe", "MQTT :1883", label_w=190, label_dx=-60)}
    <!-- Subscriptions <-> Stripe (Directo vertical) -->
    {draw_arrow(1460, 600, 1600, 1120, "Crea sesiones checkout", "HTTPS / REST", label_w=170, label_dx=40)}
    {draw_curved_arrow(1680, 1120, 1750, 850, 1500, 600, "Webhooks de cobro", "HTTPS", label_w=150)}
    <!-- Profiles -> Cloudinary (Directo vertical) -->
    {draw_arrow(2100, 600, 2030, 1120, "Sube fotos de perfil", "HTTPS / REST", label_w=160)}
    <!-- Analytics -> InfluxDB (Lado izquierdo) -->
    {draw_curved_arrow(1660, 600, 800, 990, 320, 1120, "Ingesta telemetría", "HTTP :8086", label_w=150)}

    <g transform="translate(60, 1340)">
        <rect width="900" height="60" rx="8" fill="#ffffff" stroke="#d0dbe5" stroke-width="1"/>
        <rect x="20" y="18" width="24" height="24" rx="4" fill="#438DD5"/>
        <text x="52" y="35" font-family="'Segoe UI', sans-serif" font-size="13" font-weight="600" fill="#222222">Componente de Módulo (.NET 9)</text>
        <rect x="310" y="18" width="24" height="24" rx="4" fill="#2A72C9"/>
        <text x="342" y="35" font-family="'Segoe UI', sans-serif" font-size="13" font-weight="600" fill="#222222">Contenedor Relacionado</text>
        <rect x="550" y="18" width="24" height="24" rx="4" fill="#1F618D"/>
        <text x="582" y="35" font-family="'Segoe UI', sans-serif" font-size="13" font-weight="600" fill="#222222">Base de Datos</text>
        <rect x="700" y="18" width="24" height="24" rx="4" fill="#8A9BA8"/>
        <text x="732" y="35" font-family="'Segoe UI', sans-serif" font-size="13" font-weight="600" fill="#222222">Sistema Externo</text>
    </g>
</svg>
"""
    return svg

def render_to_jpg(svg_content, html_filename, png_filename, jpg_filename, window_w, window_h):
    html_path = os.path.join(OUTPUT_DIR, html_filename)
    png_path = os.path.join(OUTPUT_DIR, png_filename)
    jpg_path = os.path.join(OUTPUT_DIR, jpg_filename)
    
    html = f"""<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8"/>
    <style>
        body {{ margin:0; padding:0; background:#f8fafd; display:flex; justify-content:center; align-items:center; }}
    </style>
</head>
<body>
    {svg_content}
</body>
</html>"""
    with open(html_path, "w", encoding="utf-8") as f:
        f.write(html)
        
    cmd = [
        EDGE_PATH,
        "--headless=new",
        "--disable-gpu",
        f"--window-size={window_w},{window_h}",
        f"--screenshot={png_path}",
        f"file:///{html_path.replace(os.sep, '/')}"
    ]
    subprocess.run(cmd, check=True)
    
    if os.path.exists(png_path):
        with Image.open(png_path) as img:
            rgb_img = img.convert("RGB")
            rgb_img.save(jpg_path, "JPEG", quality=95, optimize=True)
        print(f"Generated JPG: {jpg_path}")

if __name__ == "__main__":
    print("Rendering Nivel 1: Contexto...")
    render_to_jpg(build_context_svg(), "c4_context.html", "c4_nivel1_contexto.png", "c4_nivel1_contexto.jpg", 1850, 1150)
    
    print("Rendering Nivel 2: Contenedores...")
    render_to_jpg(build_container_svg(), "c4_container.html", "c4_nivel2_contenedores.png", "c4_nivel2_contenedores.jpg", 2450, 1450)
    
    print("Rendering Nivel 3: Componentes...")
    render_to_jpg(build_component_svg(), "c4_component.html", "c4_nivel3_componentes.png", "c4_nivel3_componentes.jpg", 2450, 1500)
    print("All C4 JPG diagrams rendered successfully!")
