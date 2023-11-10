---
title: "Bacon ipsum"
date: 2023-11-08T18:03:26+01:00
cover : "bacon-ipsum-banner1.jpg"
image : "bacon-ipsum-banner1.jpg"
useRelativeCover : true
draft: true
tags:
  - tagA
  - tagB
description: "AKHSJKahsjkAHSJKAHS"
toc : true
---

# aaa

Bacon ipsum dolor amet hamburger capicola buffalo venison, shankle pig drumstick frankfurter jerky ground round brisket doner. Doner sirloin flank picanha t-bone ham hock. Landjaeger spare ribs ground round t-bone, tri-tip ribeye bresaola buffalo pork loin pork chop meatball jerky biltong. Bacon kevin capicola, shankle tenderloin ground round pancetta venison biltong. Meatball pork belly fatback, corned beef short ribs cupim turkey salami beef ribs. Rump tongue kielbasa pig shank burgdoggen, brisket filet mignon ball tip ribeye cow venison.

## SSSS

Bacon ipsum dolor amet hamburger capicola buffalo venison, shankle pig drumstick frankfurter jerky ground round brisket doner. Doner sirloin flank picanha t-bone ham hock. Landjaeger spare ribs ground round t-bone, tri-tip ribeye bresaola buffalo pork loin pork chop meatball jerky biltong. Bacon kevin capicola, shankle tenderloin ground round pancetta venison biltong. Meatball pork belly fatback, corned beef short ribs cupim turkey salami beef ribs. Rump tongue kielbasa pig shank burgdoggen, brisket filet mignon ball tip ribeye cow venison.

$$ 5 \times 5 = 25 $$

## SSS

Bacon ipsum dolor amet hamburger capicola buffalo venison, shankle pig drumstick frankfurter jerky ground round brisket doner. Doner sirloin flank picanha t-bone ham hock. Landjaeger spare ribs ground round t-bone, tri-tip ribeye bresaola buffalo pork loin pork chop meatball jerky biltong. Bacon kevin capicola, shankle tenderloin ground round pancetta venison biltong. Meatball pork belly fatback, corned beef short ribs cupim turkey salami beef ribs. Rump tongue kielbasa pig shank burgdoggen, brisket filet mignon ball tip ribeye cow venison.


{{</* tabs tabTotal="2" */>}}

{{%/* tab tabName="First Tab" */%}}
This is markdown content.
{{%/* /tab */%}}

{{</* tab tabName="Second Tab" */>}}
{{</* highlight text */>}}
This is a code block.
{{</* /highlight */>}}
{{</* /tab */>}}

{{</* /tabs */>}}

# bbbb
askjhasdjkashdkjlh


lkasjdklsajdslakdj



```csharp
namespace SimpleActor.Core
{
	public sealed class Actor
	{
		private readonly Channel<(object, Actor)> _mailbox = Channel.CreateUnbounded<(object, Actor)>();

		private readonly Action<object, Actor> _handler;

		public Actor(Action<object, Actor> handler)
		{
			_handler = handler;

			Task.Run(Loop);
		}

		public void Tell(object message, Actor sender = null)
		{
			var pair = (message, sender);
			_mailbox.Writer.TryWrite(pair);
		}

		public void GracefulStop() => _mailbox.Writer.Complete();

		private async Task Loop()
		{
			await foreach (var pair in _mailbox.Reader.ReadAllAsync())
			{
				var (message, sender) = pair;
				_handler(message, sender);
			}
		}
	}
}

```