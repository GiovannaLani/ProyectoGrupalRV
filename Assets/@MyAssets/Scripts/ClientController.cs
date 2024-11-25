using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientController : MonoBehaviour
{
    private int lives;

    void Start()
    {
<<<<<<< Updated upstream
=======
        agent = GetComponent<NavMeshAgent>();
        pointsToVisit = Random.Range(2, patrolPoints.Count + 1);
        animator = GetComponent<Animator>();
        StartCoroutine(MoveToPoints());
    }
    IEnumerator MoveToPoints()
    {
        List<Transform> visitedPoints = new List<Transform>();

        while (isAlive)
        {
            if (isFinalMove)
            {
                currentTarget = finalPoints[Random.Range(0, finalPoints.Count)];
                agent.SetDestination(currentTarget.position);

                while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
                {
                    animator.SetBool("lookAround", false);
                    animator.SetBool("walk", true);

                    if (agent.velocity.sqrMagnitude > 0.01f)
                    {
                        Vector3 direction = agent.velocity.normalized; 
                        Quaternion lookRotation = Quaternion.LookRotation(direction * -1);
                        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f); 
                    }
                    yield return null;
                }

                animator.SetBool("walk", false);
                StopMovement();
                yield break;
            }

            if (!isGoingToBuy && Random.value < 0.5f && buyPoint != null && pointsVisited < pointsToVisit - 1)
            {
                currentTarget = buyPoint;
                waitTime = 50f;
                isGoingToBuy = true;
                pointsVisited++;
            }
            else
            {
                do
                {
                    currentTarget = patrolPoints[Random.Range(0, patrolPoints.Count)];
                } while (visitedPoints.Contains(currentTarget));

                visitedPoints.Add(currentTarget);
                pointsVisited++;
            }

            agent.SetDestination(currentTarget.position);

            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                animator.SetBool("lookAround", false);
                animator.SetBool("walk", true);

                    if (agent.velocity.sqrMagnitude > 0.01f) 
                {
                    Vector3 direction = agent.velocity.normalized;
                    Quaternion lookRotation = Quaternion.LookRotation(direction*-1);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                }
                yield return null;
            }

            animator.SetBool("walk", false);

            agent.updateRotation = true;
            animator.SetBool("lookAround",true);
            yield return new WaitForSeconds(waitTime);
            if (pointsVisited >= pointsToVisit || isGoingToBuy)
            {
                isFinalMove = true;
            }
        }
>>>>>>> Stashed changes
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Client: OnTriggerEnter");
        if(other.gameObject.GetComponent<WeaponController>())
        {
            Debug.Log("Client: OnTriggerEnter cuchillo");

            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
