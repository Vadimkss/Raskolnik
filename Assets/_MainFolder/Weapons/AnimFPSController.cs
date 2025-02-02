using UnityEngine;

public class AnimateAt24FPS : MonoBehaviour
{
    private Animator animator;
    private float timer = 0f;
    private float frameRate = 1f / 24f;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= frameRate)
        {
            timer -= frameRate; // Сбрасываем таймер, чтобы сохранить частоту
            animator.Update(frameRate); // Обновляем анимацию вручную только каждые 1/24 секунды
        }
    }
}