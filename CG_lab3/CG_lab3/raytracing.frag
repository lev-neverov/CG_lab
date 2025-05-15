#version 430

#define DIFFUSE 1
#define REFLECTION 2
#define REFRACTION 3
#define EPSILON 0.001
#define BIG 1000000.0

// ��������� ������
struct SCamera
{
    vec3 Position;
    vec3 View;
    vec3 Up;
    vec3 Side;
    vec2 Scale;
};

struct SRay
{
    vec3 Origin;
    vec3 Direction;
};

struct SSphere
{
    vec3 Center;
    float Radius;
    int MaterialIdx;
};

struct STriangle
{
    vec3 v1;
    vec3 v2;
    vec3 v3;
    int MaterialIdx;
};

struct SLight
{
    vec3 Position;
};

struct SMaterial
{
    vec3 Color;
    vec4 LightCoeffs;
    float ReflectionCoeff;
    float RefractionCoeff;
    int MaterialType;
};

struct SIntersection
{
    float Time;
    vec3 Point;
    vec3 Normal;
    vec3 Color;
    vec4 LightCoeffs;
    float ReflectionCoeff;
    float RefractionCoeff;
    int MaterialType;
};

struct STracingRay
{
    SRay ray;
    float contribution;
    int depth;
};

// �������/�������� ����������
in vec3 glPosition;
out vec4 FragColor;

// Uniform-����������
uniform vec3 campos;
uniform float aspect;

// ���������� ���������� �����
STriangle triangles[10];
SSphere spheres[2];
SMaterial materials[2];
SLight light;

// �������
SCamera initializeDefaultCamera()
{
    SCamera camera;
    camera.Position = campos;
    camera.View = vec3(0.0, 0.0, 1.0);
    camera.Up = vec3(0.0, 1.0, 0.0);
    camera.Side = vec3(1.0, 0.0, 0.0);
    camera.Scale = vec2(1.0, aspect);
    return camera;
}

SRay GenerateRay(SCamera uCamera)
{
    vec2 coords = glPosition.xy * uCamera.Scale;
    vec3 direction = uCamera.View + uCamera.Side * coords.x + uCamera.Up * coords.y;
    return SRay(uCamera.Position, normalize(direction));
}

bool IntersectSphere(SSphere sphere, SRay ray, float start, float final, out float time)
{
    ray.Origin -= sphere.Center;
    float A = dot(ray.Direction, ray.Direction);
    float B = dot(ray.Direction, ray.Origin);
    float C = dot(ray.Origin, ray.Origin) - sphere.Radius * sphere.Radius;
    float D = B * B - A * C;
    
    if(D > 0.0)
    {
        D = sqrt(D);
        float t1 = (-B - D) / A;
        float t2 = (-B + D) / A;
        
        if(t1 < 0 && t2 < 0) return false;
        if(min(t1, t2) < 0)
        {
            time = max(t1, t2);
            return true;
        }
        
        time = min(t1, t2);
        return true;
    }
    return false;
}

bool IntersectTriangle(SRay ray, vec3 v1, vec3 v2, vec3 v3, out float time)
{
    time = -1;
    vec3 A = v2 - v1;
    vec3 B = v3 - v1;
    vec3 N = cross(A, B);
    
    float NdotRayDirection = dot(N, ray.Direction);
    if(abs(NdotRayDirection) < 0.001) return false;
    
    float d = dot(N, v1);
    float t = -(dot(N, ray.Origin) - d) / NdotRayDirection;
    if(t < 0) return false;
    
    vec3 P = ray.Origin + t * ray.Direction;
    
    vec3 C;
    vec3 edge1 = v2 - v1;
    vec3 VP1 = P - v1;
    C = cross(edge1, VP1);
    if(dot(N, C) < 0) return false;
    
    vec3 edge2 = v3 - v2;
    vec3 VP2 = P - v2;
    C = cross(edge2, VP2);
    if(dot(N, C) < 0) return false;
    
    vec3 edge3 = v1 - v3;
    vec3 VP3 = P - v3;
    C = cross(edge3, VP3);
    if(dot(N, C) < 0) return false;
    
    time = t;
    return true;
}

void initializeDefaultScene(out STriangle triangles[10], out SSphere spheres[2])
{
    /* left wall */
    triangles[0].v1 = vec3(-5.0,-5.0,-5.0);
    triangles[0].v2 = vec3(-5.0, 5.0, 5.0);
    triangles[0].v3 = vec3(-5.0, 5.0,-5.0);
    triangles[0].MaterialIdx = 0;

    triangles[1].v1 = vec3(-5.0,-5.0,-5.0);
    triangles[1].v2 = vec3(-5.0,-5.0, 5.0);
    triangles[1].v3 = vec3(-5.0, 5.0, 5.0);
    triangles[1].MaterialIdx = 0;

    /* back wall */
    triangles[2].v1 = vec3(-5.0,-5.0, 5.0);
    triangles[2].v2 = vec3( 5.0,-5.0, 5.0);
    triangles[2].v3 = vec3(-5.0, 5.0, 5.0);
    triangles[2].MaterialIdx = 0;

    triangles[3].v1 = vec3( 5.0, 5.0, 5.0);
    triangles[3].v2 = vec3(-5.0, 5.0, 5.0);
    triangles[3].v3 = vec3( 5.0,-5.0, 5.0);
    triangles[3].MaterialIdx = 0;

    /* front wall */
    triangles[4].v1 = vec3(-5.0,-5.0,-5.0);
    triangles[4].v2 = vec3(5.0,-5.0,-5.0);
    triangles[4].v3 = vec3(5.0,5.0,-5.0);
    triangles[4].MaterialIdx = 0;

    triangles[5].v1 = vec3(-5.0,-5.0,-5.0);
    triangles[5].v2 = vec3(5.0,5.0,-5.0);
    triangles[5].v3 = vec3(-5.0,5.0,-5.0);
    triangles[5].MaterialIdx = 0;

    /* right wall */
    triangles[6].v1 = vec3(5.0,-5.0,-5.0);
    triangles[6].v2 = vec3(5.0,-5.0,5.0);
    triangles[6].v3 = vec3(5.0,5.0,5.0);
    triangles[6].MaterialIdx = 0;

    triangles[7].v1 = vec3(5.0,-5.0,-5.0);
    triangles[7].v2 = vec3(5.0,5.0,5.0);
    triangles[7].v3 = vec3(5.0,5.0,-5.0);
    triangles[7].MaterialIdx = 0;

    /* bottom wall */
    triangles[8].v1 = vec3(-5.0,-5.0,-5.0);
    triangles[8].v2 = vec3(5.0,-5.0,-5.0);
    triangles[8].v3 = vec3(5.0,-5.0,5.0);
    triangles[8].MaterialIdx = 0;

    triangles[9].v1 = vec3(-5.0,-5.0,-5.0);
    triangles[9].v2 = vec3(5.0,-5.0,5.0);
    triangles[9].v3 = vec3(-5.0,-5.0,5.0);
    triangles[9].MaterialIdx = 0;

    spheres[0].Center = vec3(-1.0,-1.0,-2.0);
    spheres[0].Radius = 2.0;
    spheres[0].MaterialIdx = 0;

    spheres[1].Center = vec3(2.0,1.0,2.0);
    spheres[1].Radius = 1.0;
    spheres[1].MaterialIdx = 0;
}

void initializeDefaultLightMaterials(out SLight light, out SMaterial materials[2])
{
    light.Position = vec3(0.0, 2.0, -4.0f);

    vec4 lightCoefs = vec4(0.4,0.9,0.0,512.0);
    materials[0].Color = vec3(0.0, 1.0, 0.0);
    materials[0].LightCoeffs = lightCoefs;
    materials[0].ReflectionCoeff = 0.5;
    materials[0].RefractionCoeff = 1.0;
    materials[0].MaterialType = DIFFUSE;

    materials[1].Color = vec3(0.0, 0.0, 1.0);
    materials[1].LightCoeffs = lightCoefs;
    materials[1].ReflectionCoeff = 0.5;
    materials[1].RefractionCoeff = 1.0;
    materials[1].MaterialType = DIFFUSE;
}

vec3 Phong(SIntersection intersect, SLight currLight, SCamera uCamera)
{
    vec3 light = normalize(currLight.Position - intersect.Point);
    float diffuse = max(dot(light, intersect.Normal), 0.0);
    vec3 view = normalize(uCamera.Position - intersect.Point);
    vec3 reflected = reflect(-view, intersect.Normal);
    float specular = pow(max(dot(reflected, light), 0.0), intersect.LightCoeffs.w);
    
    return intersect.LightCoeffs.x * intersect.Color +
           intersect.LightCoeffs.y * diffuse * intersect.Color +
           intersect.LightCoeffs.z * specular * vec3(1.0,1.0,1.0);
}

bool Raytrace(SRay ray, float start, float final, inout SIntersection intersect)
{
    bool result = false;
    float test = start;
    intersect.Time = final;
    
    // ����������� �� �������
    for(int i = 0; i < 2; i++)
    {
        if(IntersectSphere(spheres[i], ray, start, final, test) && test < intersect.Time)
        {
            intersect.Time = test;
            intersect.Point = ray.Origin + ray.Direction * test;
            intersect.Normal = normalize(intersect.Point - spheres[i].Center);
            intersect.Color = materials[spheres[i].MaterialIdx].Color;
            intersect.LightCoeffs = materials[spheres[i].MaterialIdx].LightCoeffs;
            intersect.ReflectionCoeff = materials[spheres[i].MaterialIdx].ReflectionCoeff;
            intersect.RefractionCoeff = materials[spheres[i].MaterialIdx].RefractionCoeff;
            intersect.MaterialType = materials[spheres[i].MaterialIdx].MaterialType;
            result = true;
        }
    }
    
    // ����������� � ��������������
    for(int i = 0; i < 10; i++)
    {
        if(IntersectTriangle(ray, triangles[i].v1, triangles[i].v2, triangles[i].v3, test) && test < intersect.Time)
        {
            intersect.Time = test;
            intersect.Point = ray.Origin + ray.Direction * test;
            intersect.Normal = normalize(cross(triangles[i].v1 - triangles[i].v2, triangles[i].v3 - triangles[i].v2));
            intersect.Color = materials[triangles[i].MaterialIdx].Color;
            intersect.LightCoeffs = materials[triangles[i].MaterialIdx].LightCoeffs;
            intersect.ReflectionCoeff = materials[triangles[i].MaterialIdx].ReflectionCoeff;
            intersect.RefractionCoeff = materials[triangles[i].MaterialIdx].RefractionCoeff;
            intersect.MaterialType = materials[triangles[i].MaterialIdx].MaterialType;
            result = true;
        }
    }
    
    return result;
}

void main(void)
{
    // ������������� �����
    initializeDefaultScene(triangles, spheres);
    initializeDefaultLightMaterials(light, materials);
    
    SCamera uCamera = initializeDefaultCamera();
    SRay ray = GenerateRay(uCamera);
    
    float start = 0;
    float final = BIG;
    vec3 resultColor = vec3(0,0,0);
    
    SIntersection intersect;
    intersect.Time = BIG;
    
    if(Raytrace(ray, start, final, intersect))
    {
        resultColor = Phong(intersect, light, uCamera);
    }
    
    FragColor = vec4(resultColor, 1.0);
}