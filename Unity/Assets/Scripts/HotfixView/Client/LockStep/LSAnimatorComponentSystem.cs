using System;
using UnityEngine;

namespace ET.Client
{
	/// <summary>
	/// LSAnimatorComponent系统类，负责管理动画组件的生命周期和动画播放
	/// </summary>
	[EntitySystemOf(typeof(LSAnimatorComponent))]
	[FriendOf(typeof(LSAnimatorComponent))]
	public static partial class LSAnimatorComponentSystem
	{
		/// <summary>
		/// 销毁动画组件时清理资源
		/// </summary>
		/// <param name="self">动画组件实例</param>
		[EntitySystem]
		private static void Destroy(this LSAnimatorComponent self)
		{
			self.animationClips = null;
			self.Parameter = null;
			self.Animator = null;
		}
		
		/// <summary>
		/// 初始化动画组件，获取Animator组件并缓存动画片段和参数
		/// </summary>
		/// <param name="self">动画组件实例</param>
		[EntitySystem]
		private static void Awake(this LSAnimatorComponent self)
		{
			Animator animator = self.GetParent<LSUnitView>().GameObject.GetComponent<Animator>();

			if (animator == null)
			{
				return;
			}

			if (animator.runtimeAnimatorController == null)
			{
				return;
			}

			if (animator.runtimeAnimatorController.animationClips == null)
			{
				return;
			}
			self.Animator = animator;
			foreach (AnimationClip animationClip in animator.runtimeAnimatorController.animationClips)
			{
				self.animationClips[animationClip.name] = animationClip;
			}
			foreach (AnimatorControllerParameter animatorControllerParameter in animator.parameters)
			{
				self.Parameter.Add(animatorControllerParameter.name);
			}
		}
		
		/// <summary>
		/// 更新动画状态，处理动画播放逻辑
		/// </summary>
		/// <param name="self">动画组件实例</param>
		[EntitySystem]
		private static void Update(this LSAnimatorComponent self)
		{
			if (self.isStop)
			{
				return;
			}

			if (self.MotionType == MotionType.None)
			{
				return;
			}

			try
			{
				self.Animator.SetFloat("MotionSpeed", self.MontionSpeed);

				self.Animator.SetTrigger(self.MotionType.ToString());

				self.MontionSpeed = 1;
				self.MotionType = MotionType.None;
			}
			catch (Exception ex)
			{
				throw new Exception($"动作播放失败: {self.MotionType}", ex);
			}
		}

		/// <summary>
		/// 检查动画器是否包含指定的参数
		/// </summary>
		/// <param name="self">动画组件实例</param>
		/// <param name="parameter">参数名称</param>
		/// <returns>如果包含该参数返回true，否则返回false</returns>
		public static bool HasParameter(this LSAnimatorComponent self, string parameter)
		{
			return self.Parameter.Contains(parameter);
		}

		/// <summary>
		/// 在指定时间内播放动画
		/// </summary>
		/// <param name="self">动画组件实例</param>
		/// <param name="motionType">动画类型</param>
		/// <param name="time">播放时间（秒）</param>
		public static void PlayInTime(this LSAnimatorComponent self, MotionType motionType, float time)
		{
			AnimationClip animationClip;
			if (!self.animationClips.TryGetValue(motionType.ToString(), out animationClip))
			{
				throw new Exception($"找不到该动作: {motionType}");
			}

			float motionSpeed = animationClip.length / time;
			if (motionSpeed < 0.01f || motionSpeed > 1000f)
			{
				Log.Error($"motionSpeed数值异常, {motionSpeed}, 此动作跳过");
				return;
			}
			self.MotionType = motionType;
			self.MontionSpeed = motionSpeed;
		}

		/// <summary>
		/// 播放指定类型的动画
		/// </summary>
		/// <param name="self">动画组件实例</param>
		/// <param name="motionType">动画类型</param>
		/// <param name="motionSpeed">动画播放速度，默认为1.0</param>
		public static void Play(this LSAnimatorComponent self, MotionType motionType, float motionSpeed = 1f)
		{
			if (!self.HasParameter(motionType.ToString()))
			{
				return;
			}
			self.MotionType = motionType;
			self.MontionSpeed = motionSpeed;
		}

		/// <summary>
		/// 获取指定动画类型的播放时长
		/// </summary>
		/// <param name="self">动画组件实例</param>
		/// <param name="motionType">动画类型</param>
		/// <returns>动画片段的长度（秒）</returns>
		public static float AnimationTime(this LSAnimatorComponent self, MotionType motionType)
		{
			AnimationClip animationClip;
			if (!self.animationClips.TryGetValue(motionType.ToString(), out animationClip))
			{
				throw new Exception($"找不到该动作: {motionType}");
			}
			return animationClip.length;
		}

		/// <summary>
		/// 暂停动画播放
		/// </summary>
		/// <param name="self">动画组件实例</param>
		public static void PauseAnimator(this LSAnimatorComponent self)
		{
			if (self.isStop)
			{
				return;
			}
			self.isStop = true;

			if (self.Animator == null)
			{
				return;
			}
			self.stopSpeed = self.Animator.speed;
			self.Animator.speed = 0;
		}

		/// <summary>
		/// 恢复动画播放
		/// </summary>
		/// <param name="self">动画组件实例</param>
		public static void RunAnimator(this LSAnimatorComponent self)
		{
			if (!self.isStop)
			{
				return;
			}

			self.isStop = false;

			if (self.Animator == null)
			{
				return;
			}
			self.Animator.speed = self.stopSpeed;
		}

		/// <summary>
		/// 设置动画器的布尔参数值
		/// </summary>
		/// <param name="self">动画组件实例</param>
		/// <param name="name">参数名称</param>
		/// <param name="state">布尔值</param>
		public static void SetBoolValue(this LSAnimatorComponent self, string name, bool state)
		{
			if (!self.HasParameter(name))
			{
				return;
			}

			self.Animator.SetBool(name, state);
		}

		/// <summary>
		/// 设置动画器的浮点参数值
		/// </summary>
		/// <param name="self">动画组件实例</param>
		/// <param name="name">参数名称</param>
		/// <param name="state">浮点值</param>
		public static void SetFloatValue(this LSAnimatorComponent self, string name, float state)
		{
			if (!self.HasParameter(name))
			{
				return;
			}

			self.Animator.SetFloat(name, state);
		}

		/// <summary>
		/// 设置动画器的整数参数值
		/// </summary>
		/// <param name="self">动画组件实例</param>
		/// <param name="name">参数名称</param>
		/// <param name="value">整数值</param>
		public static void SetIntValue(this LSAnimatorComponent self, string name, int value)
		{
			if (!self.HasParameter(name))
			{
				return;
			}

			self.Animator.SetInteger(name, value);
		}

		/// <summary>
		/// 触发动画器的触发器参数
		/// </summary>
		/// <param name="self">动画组件实例</param>
		/// <param name="name">触发器参数名称</param>
		public static void SetTrigger(this LSAnimatorComponent self, string name)
		{
			if (!self.HasParameter(name))
			{
				return;
			}

			self.Animator.SetTrigger(name);
		}

		/// <summary>
		/// 设置动画器的播放速度
		/// </summary>
		/// <param name="self">动画组件实例</param>
		/// <param name="speed">播放速度</param>
		public static void SetAnimatorSpeed(this LSAnimatorComponent self, float speed)
		{
			self.stopSpeed = self.Animator.speed;
			self.Animator.speed = speed;
		}

		/// <summary>
		/// 重置动画器的播放速度到之前保存的值
		/// </summary>
		/// <param name="self">动画组件实例</param>
		public static void ResetAnimatorSpeed(this LSAnimatorComponent self)
		{
			self.Animator.speed = self.stopSpeed;
		}
	}
}