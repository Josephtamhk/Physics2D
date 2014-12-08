﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Physics2D.Common;
using Physics2D.Object;

namespace Physics2D.Collision
{
    internal class ParticleContact
    {
        #region 公共属性
        /// <summary>
        /// 接触物体A
        /// </summary>
        public Particle PA;
        
        /// <summary>
        /// 接触物体B
        /// </summary>
        public Particle PB;

        /// <summary>
        /// 碰撞恢复系数
        /// </summary>
        public float restitution;

        /// <summary>
        /// 相交深度
        /// </summary>
        public float penetration;

        /// <summary>
        /// 碰撞法线
        /// </summary>
        public Vector2D contactNormal;
        #endregion

        #region 公开方法
        /// <summary>
        /// 解决碰撞问题
        /// 解决速度及相交
        /// </summary>
        /// <param name="duration">持续时间</param>
        public void resolve(float duration)
        {
            resolveVelocity(duration);
            resolveInterpenetration(duration);
        }

        /// <summary>
        /// 计算分离速度
        /// </summary>
        /// <returns>分离速度</returns>
        public float calculateSeparatingVelocity()
        {
            return (PA.Velocity - PB.Velocity) * contactNormal;
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 解决碰撞后速度
        /// </summary>
        /// <param name="duration"></param>
        private void resolveVelocity(float duration)
        {
            float separatingVelocity = calculateSeparatingVelocity();

            if(separatingVelocity > 0f)
            {
                // 两个物体已经分离或静止
                return;
            }

            float newSeparatingVelocity = -separatingVelocity * restitution;

            // 检查仅有加速度产生的速度
            float accCausedSeparatingVelocity = (PA.Acceleration - PB.Acceleration) * contactNormal * duration;
            if (accCausedSeparatingVelocity < 0f)
            {
                // 补偿由加速度产生的速度
                newSeparatingVelocity += restitution * accCausedSeparatingVelocity;
                // 避免过度补偿
                if (newSeparatingVelocity < 0f) newSeparatingVelocity = 0;
            }

            float deltaVelocity = newSeparatingVelocity - separatingVelocity;

            float totalInverseMass = PA.InverseMass + PB.InverseMass;

            // 两个物体全为固定或匀速物体则不处理
            if (totalInverseMass <= 0f) return;

            // 计算冲量
            float impulse = deltaVelocity / totalInverseMass;

            // 施加冲量
            Vector2D impulsePerIMass = contactNormal * impulse;
            PA.Velocity += impulsePerIMass * PA.InverseMass;
            PB.Velocity += impulsePerIMass * PB.InverseMass;
        }

        /// <summary>
        /// 解决碰撞后相交
        /// </summary>
        /// <param name="duration"></param>
        private void resolveInterpenetration(float duration)
        {
            // 对象未相交
            if (penetration <= 0f) return;

            // 不处理两个均为固定或常速运动的物体
            float totalInverseMass = PA.InverseMass + PB.InverseMass;
            if (totalInverseMass <= 0f) return;

            Vector2D movePerIMass = contactNormal * (-penetration / totalInverseMass);

            PA.Position += movePerIMass * PA.InverseMass;
            PB.Position += movePerIMass * PB.InverseMass;
        }
        #endregion
    }
}