#ifndef RANDOM_DEFINE
#define RANDOM_DEFINE

struct Random
{
    float2 seed;
    float2 random_vec;
};

float rand(inout Random random)
{
    float g = random.random_vec.x * random.random_vec.y;
    random.seed -= float2(g, g);
    return frac(sin(dot(random.seed, float2(12.9898, 78.233))) * 43758.5453);
}

float2 rand2(inout Random random)
{
    return float2(
        rand(random),
        rand(random)
        );
}



#endif // RANDOM_DEFINE
